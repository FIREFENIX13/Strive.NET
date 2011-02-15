﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using UpdateControls.XAML;

using Strive.Client.Model;
using Strive.Common;


namespace Strive.Client.ViewModel
{
    public class PerspectiveViewModel
    {
        static readonly Vector3D XAxis = new Vector3D(1, 0, 0);
        static readonly Vector3D YAxis = new Vector3D(0, 1, 0);
        static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);

        public string WindowTitle
        {
            get
            {
                if (_followEntities.Count > 0)
                    return "Following (" + _followEntities + ")";
                return "Fly free view";
            }
        }

        private EnumSkill _currentGameCommand = EnumSkill.None;
        public EnumSkill CurrentGameCommand
        {
            get { return _currentGameCommand; }
            set
            {
                _currentGameCommand = value;
                //ITexture texture = resources.GetCursor((int)currentGameCommand);
                //CurrentWorld.RenderingScene.SetCursor(texture);
            }
        }

        DictionaryModel<string, EntityModel> _followEntities = new DictionaryModel<string, EntityModel>();
        public IEnumerable<EntityViewModel> FollowEntities
        {
            get
            {
                return _followEntities.Entities
                    .Select(em => new EntityViewModel(em, WorldViewModel.Navigation));
            }
        }

        public ICommand CreateEntity
        {
            get
            {
                return MakeCommand
                    .Do(() => WorldViewModel.ServerConnection
                        .CreateMobile(1, Position, Rotation));
            }
        }

        public ICommand PossessEntity
        {
            get
            {
                return MakeCommand
                    .When(() => WorldViewModel.MouseOverEntity != null)
                    .Do(() =>
                            {
                                var id = int.Parse(WorldViewModel.MouseOverEntity.Entity.Name);
                                WorldViewModel.ServerConnection
                                    .PossessMobile(id);
                                PossessingId = id;
                            });
            }
        }

        public int PossessingId { get; private set; }

        private double _heading;
        public double Heading
        {
            get { return _heading; }
            set
            {
                _heading = value;
                while (_heading < 0)
                    _heading += 360;
                while (Heading >= 360)
                    _heading -= 360;
            }
        }

        const double DistanceRangeLow = 10;
        const double DistanceRangeHigh = 300;
        const double AngleRangeLow = -89;
        const double AngleRangeHigh = 89;

        double _tilt;
        public double Tilt
        {
            get { return _tilt; }
            set
            {
                if (value < AngleRangeLow)
                    _tilt = AngleRangeLow;
                else if (value >= AngleRangeHigh)
                    _tilt = AngleRangeHigh;
                else
                    _tilt = value;

            }
        }
         
        public Vector3D Position;
        public Quaternion Rotation;

        private int _lastTick;
        private int _frameRate;

        public int Fps { get; private set; }

        private readonly Stopwatch _movementTimer;

        public delegate bool KeyPressedCheck(Key k);

        public WorldViewModel WorldViewModel { get; private set; }

        readonly KeyPressedCheck _keyPressed;

        public PerspectiveViewModel(WorldViewModel worldViewModel, KeyPressedCheck keyPressed)
        {
            WorldViewModel = worldViewModel;
            _keyPressed = keyPressed;
            Home();
            _movementTimer = new Stopwatch();
            _movementTimer.Start();
        }

        double _landSpeed = 50.0;

        public void Check()
        {
            double deltaT = NextFrameDuration();

            Vector3D initialPosition = Position;
            Quaternion initialRotation = Rotation;

            Follow(deltaT);
            ApplyKeyBindings(deltaT);

            // Send update if required
            if (Position != initialPosition || Rotation != initialRotation)
            {
                WorldViewModel.ServerConnection.MyPosition(PossessingId, Position, Rotation);
            }
        }

        private void ApplyKeyBindings(double deltaT)
        {
            int movementPerpendicular = 0;
            int movementForward = 0;
            int movementUp = 0;
            double speedModifier = 1;
            foreach (InputBindings.KeyBinding kb in WorldViewModel.Bindings.KeyBindings
                .Where(kb => kb.KeyCombo.All(k => _keyPressed(k))))
            {
                _followEntities.Clear();

                if (kb.Action == InputBindings.KeyAction.Up)
                    movementUp++;
                else if (kb.Action == InputBindings.KeyAction.Down)
                    movementUp--;
                else if (kb.Action == InputBindings.KeyAction.Left)
                    movementPerpendicular++;
                else if (kb.Action == InputBindings.KeyAction.Right)
                    movementPerpendicular--;
                else if (kb.Action == InputBindings.KeyAction.TurnLeft)
                    Heading += deltaT * 200;
                else if (kb.Action == InputBindings.KeyAction.TurnRight)
                    Heading -= deltaT * 200;
                else if (kb.Action == InputBindings.KeyAction.TiltUp)
                    Tilt -= deltaT * (AngleRangeHigh - AngleRangeLow) / 2.0;
                else if (kb.Action == InputBindings.KeyAction.TiltDown)
                    Tilt += deltaT * (AngleRangeHigh - AngleRangeLow) / 2.0;
                else if (kb.Action == InputBindings.KeyAction.Walk)
                    speedModifier = 0.2;
                else if (kb.Action == InputBindings.KeyAction.Forward)
                    movementForward++;
                else if (kb.Action == InputBindings.KeyAction.Back)
                    movementForward--;
                else if (kb.Action == InputBindings.KeyAction.Home)
                    Home();
                else if (kb.Action == InputBindings.KeyAction.FollowSelected)
                    OnFollowSelected();
                else
                    throw new Exception("Unexpected keyboard binding " + kb.Action);
            }

            // Set Position and Rotation
            Rotation = new Quaternion(ZAxis, Heading) * new Quaternion(YAxis, Tilt);

            if (movementPerpendicular != 0 || movementForward != 0 || movementUp != 0)
            {
                var transformHeading = new Matrix3D();
                transformHeading.Rotate(new Quaternion(ZAxis, Heading));

                var changeInPosition = new Vector3D(
                    movementForward * _landSpeed,
                    movementPerpendicular * _landSpeed,
                    movementUp * (DistanceRangeHigh - DistanceRangeLow) / 10.0);

                Position += changeInPosition * transformHeading * deltaT * speedModifier;

                if (Position.Z < DistanceRangeLow)
                    Position.Z = DistanceRangeLow;
                else if (Position.Z > DistanceRangeHigh)
                    Position.Z = DistanceRangeHigh;
            }
        }

        /// <summary> Move toward or follow one or more entities </summary>
        private void Follow(double deltaT)
        {
            if (_followEntities.Count > 0)
            {
                Vector3D center = _followEntities.Entities
                    .Where(e => WorldViewModel.WorldModel.ContainsKey(e.Name))
                    .Average(e => e.Position);
                Vector3D diff = center - Position;
                double vectorDistance = diff.Length;

                // TODO: replace with a proper bounds and fulstrum calculation
                var maxX = _followEntities.Entities.Max(e => e.Position.X);
                var maxY = _followEntities.Entities.Max(e => e.Position.Y);
                var maxZ = _followEntities.Entities.Max(e => e.Position.Z);
                var minX = _followEntities.Entities.Min(e => e.Position.X);
                var minY = _followEntities.Entities.Min(e => e.Position.Y);
                var minZ = _followEntities.Entities.Min(e => e.Position.Z);
                var viewDistance = new List<double> { 10.0, maxX - minX, maxY - minY, maxZ - minZ }.Max();

                Vector3D target = center - (diff * viewDistance / vectorDistance);
                Position += (target - Position) * deltaT * 2;
            }
        }

        private double NextFrameDuration()
        {
            if (Environment.TickCount - _lastTick >= 1000)
            {
                Fps = _frameRate;
                _frameRate = 0;
                _lastTick = Environment.TickCount;
            }
            _frameRate++;

            _movementTimer.Stop();
            double deltaT = _movementTimer.Elapsed.TotalSeconds;
            _movementTimer.Reset();
            _movementTimer.Start();
            return deltaT;
        }

        void Home()
        {
            Position = new Vector3D(0, 0, 23);
            Tilt = 0;
            Heading = 45;
            _followEntities.Clear();
        }

        void OnFollowSelected()
        {
            var target = WorldViewModel.Navigation.MouseOverEntity;
            if ( target != null)
            {
                _followEntities.Clear();
                _followEntities.AddEntity(target.Name, target);
                WorldViewModel.Select(target.Name);
            }
            else
                _followEntities = new DictionaryModel<string, EntityModel>(
                    WorldViewModel.Navigation.SelectedEntities
                    .Select(e => new KeyValuePair<string, EntityModel>(e.Name, e)));
        }
    }
}
