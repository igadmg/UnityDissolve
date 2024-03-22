using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SystemEx;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;


namespace UnityDissolve {
	public static class GameObjectEx {
		public static T GetComponentOrThis<T>(this GameObject o) {
			return (T)o.GetComponentOrThis(typeof(T));
		}

		public static object GetComponentOrThis(this GameObject o, Type type) {
			if (type != typeof(GameObject))
				return o.GetComponent(type);
			else
				return o;
		}

		public static object GetComponentOrAdd(this GameObject o, Type type) {
			if (type != typeof(GameObject)) {
				var c = o.GetComponent(type);
				if (c == null)
					c = o.AddComponent(type);
				return c;
			}
			else
				return o;
		}

		class GameObjectTree {
			public enum TreeStatus {
				Empty,
				Partial,
				Full
			}

			public WeakReference<GameObject> go;
			public TreeStatus Status = TreeStatus.Empty;
			public Dictionary<string, WeakReference<GameObject>> Children = new();

			public void BuildTree(ArraySegment<string> path) {
				if (go.TryGetTarget(out var root)) {
					for (var i = 0; i < path.Count; i++) {
						var child = root.transform.Find(path[i]);
						if (child == null)
							return;

						root = child.gameObject;
						Children[path.Slice(0, i).Join('/')] = root.weak();
					}

					IEnumerable<(Transform transform, string path)> scanFn(GameObject go, string path) => go.transform.Cast<Transform>().Select(t => (t, path));

					Status = path.Count == 0 ? TreeStatus.Full : TreeStatus.Partial;
					var scanList = scanFn(root, path.Join('/')).ToList();
					while (scanList.Count > 0) {
						var item = scanList.Pop();
						var itemPath = item.path.IsNullOrWhiteSpace() ? item.transform.name : string.Join('/', item.path, item.transform.name);
						Children[itemPath] = item.transform.gameObject.weak();
						scanList.AddRange(scanFn(item.transform.gameObject, itemPath));
					}
				}
			}

			public IEnumerable<GameObject> Find(string path) {
				if (path.Contains("*")) {
					var re = new Regex(path.Replace("**", ".+?").Replace("*", "[^/]+?"));
					foreach (var kvp in Children) {
						if (re.Match(kvp.Key).Success && kvp.Value.TryGetTarget(out var fgo)) {
							yield return fgo;
						}
					}
				}
				else {
					if (Children.TryGetValue(path, out var rwgo) && rwgo.TryGetTarget(out var fgo))
						yield return fgo;
				}
			}
		}

		static ConditionalWeakTable<GameObject, GameObjectTree> gameObjectTrees = new();

		public static IEnumerable<GameObject> FindGameObject(this GameObject go, string name) {
			var path = name.ToPath().SimplifyPath();
			for (int i = 0; i < path.Length; i++) {
				if (path[i] == "..") {
					go = go.transform.parent.Elvis(t => t.gameObject);
					if (go == null)
						yield break;
				}
				else {
					path = path.Skip(i);
					break;
				}
			}

			var got = gameObjectTrees.GetValue(go, go => new GameObjectTree { go = go.weak() });
			if (got.Status == GameObjectTree.TreeStatus.Full) {
				foreach (var fgo in got.Find(path.FromPath()))
					yield return fgo;
			}
			else {
				if (got.Status == GameObjectTree.TreeStatus.Empty)
					foreach (var fgo in go.FindGameObject(path, got))
						yield return fgo;
				else {
					var fgol = got.Find(path.FromPath()).ToList();
					if (fgol.Count > 0)
						foreach (var fgo in fgol)
							yield return fgo;
					else
						foreach (var fgo in go.FindGameObject(path, got))
							yield return fgo;
				}
			}
		}

		static IEnumerable<GameObject> FindGameObject(this GameObject go, ArraySegment<string> path, GameObjectTree got) {
			if (path.IsEmptyPath()) {
				yield return go;
				yield break;
			}

			var root = go;
			for (int i = 0; i < path.Count; i++) {
				var pathi = path[i];
				if (pathi == ".") {
					continue;
				}

				if (pathi == "..") {
					if (root == null)
						yield break;

					var pgo = root.transform.parent.Elvis(t => t.gameObject);
					if (pgo != null) {
						foreach (var fgo in pgo.FindGameObject(path.Slice(i + 1), i == 0 ? null : got))
							yield return fgo;
					}
					yield break;
				}

				if (pathi.IsNullOrWhiteSpace()) {
					if (i == 0) {
						root = null;
					}
					else {
						Debug.LogWarningFormat("Empty path found. Skipping. [{0}]", path.FromPath());
					}
					continue;
				}

				if (pathi.Contains("*")) {
					if (got == null) {
						got = gameObjectTrees.GetValue(root, go => new GameObjectTree { go = go.weak() });
					}
					got.BuildTree(path.Slice(0, i));

					var re = new Regex(path.FromPath().Replace("**", ".+?").Replace("*", "[^/]+?"));
					foreach (var kvp in got.Children) {
						if (re.Match(kvp.Key).Success && kvp.Value.TryGetTarget(out var fgo)) {
							yield return fgo;
						}
					}
					yield break;
				}

				if (root == null) {
					root = GameObject.Find("/" + pathi);
					if (root == null)
						yield break;
				}
				else {
					root = root.transform.Find(pathi).Elvis(t => t.gameObject);
					if (got == null) {
						got = gameObjectTrees.GetValue(root, go => new GameObjectTree { go = go.weak() });
					}
					if (got.Status != GameObjectTree.TreeStatus.Full) got.Status = GameObjectTree.TreeStatus.Partial;
					got.Children[path.Slice(0, i + 1).FromPath()] = root.weak();
				}
			}

			if (root != null)
				yield return root;

			Debug.LogWarningFormat("No child GameObject '{0}' found.", path.FromPath());
		}
	}
}
