using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;
using SystemEx;

namespace UnityDissolve.Test
{
	[TestFixture]
	public class DissolvedTypeTest
	{
		private IComparer DissolvedTypeItemComparer = LambdaComparer.Create(
					(Tuple<string, FieldInfo> a, Tuple<string, FieldInfo> b) => {
						return string.Compare(a.Item1, b.Item2.Name);
					}
				);

		[Test]
		public void AttributedClass()
		{
			var dt = new DissolvedType(typeof(SimpleDissolveClass));

			Assert.That(dt.ComponentFields, Has.Count.EqualTo(3));
			Assert.That(dt.AddComponentFields, Has.Count.EqualTo(0));
			Assert.That(dt.SubComponents, Has.Count.EqualTo(0));

			Assert.That(dt.ComponentFields, Has.Exactly(1).EqualTo(Tuple.Create<string, FieldInfo>("gameObject", null)).Using(DissolvedTypeItemComparer));
			Assert.That(dt.ComponentFields, Has.Exactly(1).EqualTo(Tuple.Create<string, FieldInfo>("meshFilter", null)).Using(DissolvedTypeItemComparer));
			Assert.That(dt.ComponentFields, Has.Exactly(1).EqualTo(Tuple.Create<string, FieldInfo>("meshCollider", null)).Using(DissolvedTypeItemComparer));

			Assert.That(dt.ComponentFields, Has.None.EqualTo(Tuple.Create<string, FieldInfo>("ommitedFieldString", null)).Using(DissolvedTypeItemComparer));
			Assert.That(dt.ComponentFields, Has.None.EqualTo(Tuple.Create<string, FieldInfo>("ommitedFieldInt", null)).Using(DissolvedTypeItemComparer));
		}
	}
}