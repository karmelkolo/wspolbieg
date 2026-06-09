namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector, 20.0);
            Assert.AreEqual(0.0, newInstance.Position.x);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(5.0, 5.0), 20.0);
            IVector curentPosition = new Vector(0.0, 0.0);

            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => {
                Assert.IsNotNull(sender);
                curentPosition = position;
                numberOfCallBackCalled++;
            };

            newInstance.Move(0.02);

            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual(15.0, curentPosition.x);
            Assert.AreEqual(15.0, curentPosition.y);
        }

        [TestMethod]
        public void Move_CalledMultipleTimes_RaisesEventExactNumberOfTimes()
        {
            Vector startPosition = new Vector(0, 0);
            Vector velocity = new Vector(1, 1);
            Ball ball = new Ball(startPosition, velocity, 20.0);

            int invocationCount = 0; 

            ball.NewPositionNotification += (sender, position) =>
            {
                invocationCount++;
            };

            int numberOfMoves = 7;

            for (int i = 0; i < numberOfMoves; i++)
            {
                ball.Move(0.016);
            }

            Assert.AreEqual(numberOfMoves, invocationCount,
                $"Błąd! Oczekiwano {numberOfMoves} wywołań eventu, a było {invocationCount}.");
        }
    }
}