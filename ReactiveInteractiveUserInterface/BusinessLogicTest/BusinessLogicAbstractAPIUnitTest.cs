//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicAbstractAPIUnitTest
    {
        [TestMethod]
        public void BusinessLogicConstructorTestMethod()
        {
            BusinessLogicAbstractAPI instance1 = BusinessLogicAbstractAPI.GetBusinessLogicLayer();
            BusinessLogicAbstractAPI instance2 = BusinessLogicAbstractAPI.GetBusinessLogicLayer();
            Assert.AreSame(instance1, instance2);
            instance1.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
        }

        [TestMethod]
        public void GetDimensionsTestMethod()
        {
            var actual = BusinessLogicAbstractAPI.GetDimensions;
            Assert.IsNotNull(actual);
            Assert.AreEqual(10.0, actual.TableWidth, "Szerokość się nie zgadza");
            Assert.AreEqual(400.0, actual.TableHeight, "Wysokość się nie zgadza");
            Assert.AreEqual(800.0, actual.BallDimension, "Rozmiar kuli się nie zgadza");
        }
    }
}