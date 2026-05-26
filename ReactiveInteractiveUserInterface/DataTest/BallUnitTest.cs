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

            newInstance.Move();

            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            // Kula przesunęła się o wektor (5,5)
            Assert.AreEqual(15.0, curentPosition.x);
            Assert.AreEqual(15.0, curentPosition.y);
        }

        // TESTY StayInBoundsTestMethod ORAZ BallDVDBounceTest USUNIĘTE: 
        // Odbijaniem od ścian zajmuje się teraz DataImplementation, a nie Ball.
    }
}