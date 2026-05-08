//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;
using DataIBall = TP.ConcurrentProgramming.Data.IBall;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
            CollisionTimer = new Timer(CheckCollision, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            CollisionTimer.Dispose();
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, int borderWidth, int borderHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall)), borderWidth, borderHeight);
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;

        private readonly Timer CollisionTimer;

        private readonly object _collisionLock = new object();

        private void CheckCollision(object? x)
        {
                var balls = layerBellow.GetBalls();

                BoundingBox boundary = new BoundingBox(200, 210, 200, 210);
                Tree tree = new Tree(boundary, 4);

                foreach (var ball in balls)
                {
                    tree.Insert(ball);
                }

                foreach (var ball1 in balls)
                {
                    BoundingBox range = new BoundingBox(ball1.Position.x + 10, ball1.Position.y + 10, 20, 20);

                    List<DataIBall> foundBalls = new List<DataIBall>();
                    tree.Search(range, foundBalls);

                    foreach (var ball2 in foundBalls)
                    {
                        if (ball1 == ball2)
                        {
                            continue;
                        }

                        double distanceX = ball2.Position.x - ball1.Position.x;
                        double distanceY = ball2.Position.y - ball1.Position.y;
                        double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                        if (distance <= 20)
                        {
                            lock (_collisionLock)
                            {

                                if (distance == 0) return; // Zabezpieczenie przed dzieleniem przez zero

                                // 3. Normalizujemy wektor (sprowadzamy jego długość do 1)
                                distanceX /= distance;
                                distanceY /= distance;

                                // 4. Wyliczamy wektor prędkości względnej
                                double dvX = ball1.Velocity.x - ball2.Velocity.x;
                                double dvY = ball1.Velocity.y - ball2.Velocity.y;

                                // 5. Iloczyn skalarny prędkości i wektora normalnego 
                                // (Określa, jak szybko kule zbliżają się do siebie wzdłuż osi uderzenia)
                                double dotProduct = dvX * distanceX + dvY * distanceY;

                                // Jeśli kule się oddalają (dotProduct < 0), nie robimy nic, 
                                // aby zapobiec "sklejaniu się" kul w kolejnych klatkach.
                                if (dotProduct < 0) return;

                                // 6. Fizyka mas - założenie: każda kula ma masę 1 (chyba że dodałeś Mass do IBall)
                                double mass1 = 1.0;
                                double mass2 = 1.0;

                                // Skalar impulsu (J)
                                double impulse = (2 * dotProduct) / (mass1 + mass2);

                                // 7. Aplikujemy impuls do prędkości obu kul
                                double newV1X = ball1.Velocity.x - impulse * mass2 * distanceX;
                                double newV1Y = ball1.Velocity.y - impulse * mass2 * distanceY;

                                double newV2X = ball2.Velocity.x + impulse * mass1 * distanceX;
                                double newV2Y = ball2.Velocity.y + impulse * mass1 * distanceY;

                                // 8. Przypisujemy nowe wektory (Tutaj wymagany jest set w interfejsie Velocity!)
                                ball1.Velocity = new LogicVector(newV1X, newV1Y);
                                ball2.Velocity = new LogicVector(newV2X, newV2Y);
                            }
                        }
                    }
                }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}