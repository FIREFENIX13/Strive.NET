﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media.Media3D;
using Strive.DataModel;

namespace Strive.Client.Model.Tests
{
    [TestClass]
    public class RecordedMapModelTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var world = new RecordedMapModel<string, EntityModel>();
            var foo = new EntityModel("Rich", "bar.3ds", new Vector3D(0, 0, 0), Quaternion.Identity);
            var bar = new EntityModel("Ada", "bar.3ds", new Vector3D(1, 1, 1), Quaternion.Identity);
            world.Set(foo.Name, foo);
            world.Set(bar.Name, bar);
            Assert.AreEqual(0, world.Get(foo.Name).Position.X);
            world.Set(foo.Name, new EntityModel(foo.Name, foo.ModelId, new Vector3D(2,2,2), Quaternion.Identity));
            Assert.AreEqual(2, world.Get(foo.Name).Position.X);
            Assert.AreEqual(3, world.CurrentVersion);
        }
    }
}