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

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, double diameter)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Diameter = diameter;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Position
        {
            get { return _position; }
            set { _position = (Vector)value; }
        }
        public IVector Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public double Diameter { get; init; }

        #endregion IBall

        #region private

        private Vector _position;
        private IVector _velocity;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move(double deltaTime)
        {
            double multiplier = deltaTime * 50;
            _position = new Vector(_position.x + _velocity.x * multiplier, _position.y + _velocity.y * multiplier);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}

