//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            BusinessLogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }, 0, 0));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); },
                  0,
                  0
                  );
                Assert.AreEqual<int>(1, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }

        [TestMethod]
        public void ThreeBallsCollisionTest()
        {
            DataAbstractAPI dataLayer = DataAbstractAPI.GetDataLayer();
            BusinessLogicImplementation logicLayer = new(dataLayer);
            logicLayer.Start(3, (pos, ball) => { }, 50000, 50000);

            var dataBalls = dataLayer.GetBalls().ToList();

            dataBalls[0].Position = new TestVector(250, 280);
            dataBalls[1].Position = new TestVector(224, 235);
            dataBalls[2].Position = new TestVector(276, 235);


            TestVector oldVector1 = new TestVector(0, -5);
            TestVector oldVector2 = new TestVector(4, 3);
            TestVector oldVector3 = new TestVector(-4, 3);

            dataBalls[0].Velocity = oldVector1;
            dataBalls[1].Velocity = oldVector2;
            dataBalls[2].Velocity = oldVector3;

            Thread.Sleep(150);

            Assert.IsTrue(oldVector1.x != dataBalls[0].Velocity.x || oldVector1.y != dataBalls[0].Velocity.y);
            Assert.IsTrue(oldVector2.x != dataBalls[1].Velocity.x || oldVector2.y != dataBalls[1].Velocity.y);
            Assert.IsTrue(oldVector3.x != dataBalls[2].Velocity.x || oldVector3.y != dataBalls[2].Velocity.y);
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, int borderWidth, int borderHeight)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Data.IBall> GetBalls()
            {
                return new List<Data.IBall>();
            }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, int borderWidth, int borderHeight)
            {
                throw new NotImplementedException();
            }
            public override IEnumerable<Data.IBall> GetBalls()
            {
                return new List<Data.IBall>();
            }
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, int borderWidth, int borderHeight)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
            }

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            private class DataBallFixture : Data.IBall
            {
                public IVector Position { get; set; } = new DataVectorFixture();
                public IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                public event EventHandler<IVector>? NewPositionNotification = null;
            }

            public override IEnumerable<Data.IBall> GetBalls()
            {
                return new List<Data.IBall>();
            }
        }

        private record TestVector(double x, double y) : Data.IVector;

        #endregion testing instrumentation
    }
}