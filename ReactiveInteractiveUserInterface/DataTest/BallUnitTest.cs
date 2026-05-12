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
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector, 0, 0);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), 100, 100);
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
            newInstance.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual<IVector>(initialPosition, curentPosition);
        }

        [TestMethod]
        public void StayInBoundsTestMethod()
        {
            Vector initialPositionTopLeft = new(0, 0);
            Vector initialPositionBottomRight = new(100, 100);
            Ball TopBall = new(initialPositionTopLeft, new Vector(0, -5), 100, 100);
            Ball LeftBall = new(initialPositionTopLeft, new Vector(-5, 0), 100, 100);
            Ball BottomBall = new(initialPositionBottomRight, new Vector(0, 5), 100, 100);
            Ball RightBall = new(initialPositionBottomRight, new Vector(5, 0), 100, 100);
            IVector TopBallPosition = new Vector(0.0, 0.0);
            IVector LeftBallPosition = new Vector(0.0, 0.0);
            IVector BottomBallPosition = new Vector(0.0, 0.0);
            IVector RightBallPosition = new Vector(0.0, 0.0);
            TopBall.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender);  TopBallPosition = position; };
            LeftBall.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); LeftBallPosition = position; };
            BottomBall.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); BottomBallPosition = position; };
            RightBall.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); RightBallPosition = position; };
            TopBall.Move();
            LeftBall.Move();
            BottomBall.Move();
            RightBall.Move();
            Assert.IsTrue(TopBallPosition.y >= 0);
            Assert.IsTrue(LeftBallPosition.x >= 0);
            Assert.IsTrue(BottomBallPosition.y <= 100);
            Assert.IsTrue(RightBallPosition.x <= 100);
        }

        [TestMethod]
        public void BallDVDBounceTest()
        {
            int boardWidth = 400;
            int boardHeight = 400;

            Vector startPosition = new Vector(390, 390);

            Vector startVelocity = new Vector(5, 5);

            Ball ball = new Ball(startPosition, startVelocity, boardWidth, boardHeight);

            ball.Move();

            Vector expectedVelocity = new Vector(-5, -5);

            Assert.AreEqual(expectedVelocity.x, ball.Velocity.x, "Odbicie w osi X nie zadziałało poprawnie!");
            Assert.AreEqual(expectedVelocity.y, ball.Velocity.y, "Odbicie w osi Y nie zadziałało poprawnie!");
        }
    }
}