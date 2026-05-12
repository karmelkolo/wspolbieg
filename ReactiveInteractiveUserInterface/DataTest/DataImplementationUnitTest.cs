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

                data.Start(10, (pos, ball) => { balls.Add(ball); }, 30, 30);

                List<double> startingVelocitiesX = new List<double>();
                foreach (var b in balls) startingVelocitiesX.Add(b.Velocity.x);

                var moveMethod = data.GetType().GetMethod("Move", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                for (int i = 0; i < 10; i++)
                {
                    moveMethod.Invoke(data, new object[] { null });
                }

                int changedVelocitiesCount = 0;
                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i].Velocity.x != startingVelocitiesX[i])
                    {
                        changedVelocitiesCount++;
                    }
                }

                Assert.IsTrue(changedVelocitiesCount >= 3, $"Karambol zawiódł. Tylko {changedVelocitiesCount} kulek zmieniło wektor.");
            }
        }

        [TestMethod]
        public void Performance_BallsMoveExpectedDistanceOverTime()
        {
            using (DataImplementation data = new DataImplementation())
            {
                List<IBall> balls = new List<IBall>();
                data.Start(1, (pos, ball) => { balls.Add(ball); }, 400, 400);

                double startX = balls[0].Position.x;
                double startY = balls[0].Position.y;

                var moveMethod = data.GetType().GetMethod("Move", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                for (int i = 0; i < 20; i++)
                {
                    moveMethod.Invoke(data, new object[] { null });
                }

                double endX = balls[0].Position.x;
                double endY = balls[0].Position.y;
                double distanceMoved = Math.Abs(endX - startX) + Math.Abs(endY - startY);

                if (balls[0].Velocity.x == 0 && balls[0].Velocity.y == 0)
                {
                    Assert.Inconclusive("Kulka wylosowała wektor (0,0) - ponów test.");
                    return;
                }

                Assert.IsTrue(distanceMoved > 0, "Kulka w ogóle się nie poruszyła pomimo wywołania 20 klatek!");
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

                Assert.IsTrue(elapsedMs < 200.0,
                    $"Oczekiwano, że 10k iteracji w sekcji krytycznej zajmie poniżej 200ms. Rzeczywisty czas: {elapsedMs:F2} ms");
            }
        }
    }
}