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
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
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
                  }
                  );
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }


        [TestMethod]
        public void Performance_BallsMoveExpectedDistanceOverTime()
        {
            using (DataImplementation data = new DataImplementation())
            {
                List<IBall> balls = new List<IBall>();
                data.Start(1, (pos, ball) => { balls.Add(ball); });

                System.Threading.Thread.Sleep(50);

                double startX = balls[0].Position.x;
                double startY = balls[0].Position.y;

                int maxAttempts = 50;
                double distanceMoved = 0;

                while (maxAttempts > 0)
                {
                    double currentX = balls[0].Position.x;
                    double currentY = balls[0].Position.y;

                    distanceMoved = Math.Abs(currentX - startX) + Math.Abs(currentY - startY);

                    if (distanceMoved > 5.0)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(10);
                    maxAttempts--;
                }

                if (balls[0].Velocity.x == 0 && balls[0].Velocity.y == 0)
                {
                    Assert.Inconclusive("Kulka wylosowała wektor (0,0) - ponów test.");
                    return;
                }

                Assert.IsTrue(distanceMoved > 5.0, "Kulka w ogóle się nie poruszyła lub poruszyła się za mało!");
            }
        }

        [TestMethod]
        public void Performance_CriticalSectionExecutionTime_IsOptimal()
        {
            double dummy = 0;
            for (int i = 0; i < 100000; i++) { dummy += Math.Sin(i); }

            using (DataImplementation data = new DataImplementation())
            {
                data.Start(1, (pos, ball) => { });

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                data.Start(10000, (pos, ball) => { });

                sw.Stop();

                double elapsedMs = sw.Elapsed.TotalMilliseconds;

                Assert.IsTrue(elapsedMs < 200.0,
                    $"Oczekiwano, że 10k iteracji w sekcji krytycznej zajmie poniżej 200ms. Rzeczywisty czas: {elapsedMs:F2} ms");
            }
        }

        [TestMethod]
        public async Task MoveBall_RealTimeExecution_CallsMoveExpectedNumberOfTimes()
        {
            int moveCount = 0;

            using (DataImplementation data = new DataImplementation())
            {
                data.Start(1, (pos, ball) =>
                {
                    ball.NewPositionNotification += (sender, args) =>
                    {
                        moveCount++;
                    };
                });

                await Task.Delay(500);
            }


            Assert.IsTrue(moveCount >= 15, $"Pętla wykonała się za rzadko! Ilość wywołań w 500ms: {moveCount}. Sprawdź Task.Delay.");
            Assert.IsTrue(moveCount <= 45, $"Pętla wykonała się za często! Ilość wywołań w 500ms: {moveCount}. Brakuje opóźnienia.");
        }

        [TestClass]
        public class LoggerUnitTest
        {
            [TestMethod]
            public async Task Logger_ConcurrentWrites_ShouldNotLoseData()
            {
                string tempFilePath = Path.GetTempFileName();
                int numberOfTasks = 50;
                int logsPerTask = 50;
                int expectedTotalLogs = numberOfTasks * logsPerTask;

                DummyBall dummyBall = new DummyBall();

                try
                {
                    using (var logger = new Logger(tempFilePath))
                    {
                        List<Task> tasks = new List<Task>();
                        for (int i = 0; i < numberOfTasks; i++)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                for (int j = 0; j < logsPerTask; j++)
                                {
                                    logger.Log(dummyBall);
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);
                        await Task.Delay(500);

                    } 
                    string[] fileLines = File.ReadAllLines(tempFilePath);
                    Assert.AreEqual(expectedTotalLogs, fileLines.Length,
                        "Logger zgubił dane przy wielowątkowości! Sprawdź implementację kolejki.");
                }
                finally
                {
                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                }
            }

            [TestMethod]
            public void Logger_Dispose_ClosesFileAndFlushesBuffer()
            {
                string tempFilePath = Path.GetTempFileName();
                try
                {
                    var logger = new Logger(tempFilePath);
                    logger.Log(new DummyBall());

                    logger.Dispose();

                    string[] lines = File.ReadAllLines(tempFilePath);

                    Assert.AreEqual(1, lines.Length, "Dispose nie zrzucił danych na dysk lub nie zamknął pliku!");
                }
                finally
                {
                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                }
            }

            #region Instrumentacja Testowa

            private class DummyBall : IBall
            {
                public IVector Position { get; set; } = new DummyVector(10, 20);
                public IVector Velocity { get; set; } = new DummyVector(0, 0);
                public double Diameter { get; } = 20.0;
                public event EventHandler<IVector>? NewPositionNotification;

                public void Dispose() { }
            }

            private record DummyVector(double x, double y) : IVector;

            #endregion
        }
    }
}