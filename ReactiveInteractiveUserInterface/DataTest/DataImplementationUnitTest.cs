//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = null;
                newInstance.CheckBallsList(x => ballsList = x);
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            bool newInstanceDisposed = false;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }, 0, 0));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsNotNull(ball);
                  },
                  100,
                  100
                  );
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }

        [TestMethod]
        public void ThreeBallsCollision_Simultaneous_ChangesVelocities()
        {
            using (DataImplementation data = new DataImplementation())
            {
                List<IBall> balls = new List<IBall>();

                data.Start(3, (pos, ball) => { balls.Add(ball); }, 21, 21);

                double v0_startX = balls[0].Velocity.x;
                double v1_startX = balls[1].Velocity.x;
                double v2_startX = balls[2].Velocity.x;

                System.Threading.Thread.Sleep(100);

                int changedVelocitiesCount = 0;

                if (balls[0].Velocity.x != v0_startX) changedVelocitiesCount++;
                if (balls[1].Velocity.x != v1_startX) changedVelocitiesCount++;
                if (balls[2].Velocity.x != v2_startX) changedVelocitiesCount++;

                Assert.IsTrue(changedVelocitiesCount >= 2, "Gwarantowany karambol 3 kul nie zadziałał! Algorytm nie zmienił wektorów.");
            }
        }

        [TestMethod]
        public void Performance_BallsMoveExpectedDistanceOverTime()
        {
            double dummy = 0;
            for (int i = 0; i < 100000; i++) { dummy += Math.Sqrt(i); }

            using (DataImplementation data = new DataImplementation())
            {
                List<IBall> balls = new List<IBall>();

                data.Start(1, (pos, ball) => { balls.Add(ball); }, 400, 400);
                System.Threading.Thread.Sleep(50); 

                double startX = balls[0].Position.x;

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                System.Threading.Thread.Sleep(300);

                sw.Stop();

                double endX = balls[0].Position.x;
                double distanceMoved = Math.Abs(endX - startX);

                Assert.IsTrue(distanceMoved > 0, "Kulka w ogóle się nie poruszyła podczas testu wydajnościowego!");

                Assert.IsTrue(distanceMoved > 5.0, $"Kulka poruszyła się za wolno w czasie {sw.ElapsedMilliseconds} ms!");
            }
        }

        [TestMethod]
        public void Performance_CriticalSectionExecutionTime_IsOptimal()
        {
            double dummy = 0;
            for (int i = 0; i < 100000; i++) { dummy += Math.Sin(i); }

            using (DataImplementation data = new DataImplementation())
            {
                data.Start(1, (pos, ball) => { }, 100, 100);

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                data.Start(10000, (pos, ball) => { }, 1000, 1000);

                sw.Stop();

                double elapsedMs = sw.Elapsed.TotalMilliseconds;

                Assert.IsTrue(elapsedMs < 50.0,
                    $"Oczekiwano, że 10k iteracji w sekcji krytycznej zajmie poniżej 50ms. Rzeczywisty czas: {elapsedMs:F2} ms");
            }
        }
    }
}