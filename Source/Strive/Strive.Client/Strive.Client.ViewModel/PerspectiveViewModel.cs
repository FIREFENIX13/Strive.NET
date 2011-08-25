﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Strive.Common;
using UpdateControls.XAML;


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

        HashSet<int> _followEntities = new HashSet<int>();

        public void UnFollow()
        {
            _followEntities.Clear();
        }

        // TODO: use the application random somehow
        private Random rand = new Random();
        public ICommand CreateEntity
        {
            get
            {
                return MakeCommand
                    .Do(() => WorldViewModel.ServerConnection.CreateEntity(
                        rand.Next(), "Junk", "StaticBox", Position, Rotation));
            }
        }

        public ICommand CreateFactory
        {
            get
            {
                return MakeCommand
                    .Do(() => WorldViewModel.ServerConnection.CreateEntity(
                        rand.Next(), "Factory", "RTSFactory", Position, Rotation));
            }
        }

        public ICommand CreateMobile
        {
            get
            {
                return MakeCommand
                    .Do(() => WorldViewModel.ServerConnection.CreateEntity(
                        rand.Next(), "Robot", "RTSRobot", Position, Rotation));
            }
        }

        public ICommand CreateMission
        {
            get
            {
                return MakeCommand
                    .When(() => WorldViewModel.WorldNavigation.SelectedEntities.Any() && WorldViewModel.IsMouseOverEntity)
                    .Do(() => WorldViewModel.CreateMission.Execute(null));
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
                                var mo = WorldViewModel.MouseOverEntity;
                                if (mo != null)
                                {
                                    WorldViewModel.ServerConnection.PossessMobile(mo.Entity.Id);
                                    PossessingId = mo.Entity.Id;
                                }
                            });
            }
        }

        public ICommand Home
        {
            get
            {
                return MakeCommand
                    .Do(() =>
                    {
                        Position = new Vector3D(0, 0, 23);
                        Tilt = 0;
                        Heading = 45;
                        UnFollow();
                    });
            }
        }

        public ICommand FollowSelected
        {
            get
            {
                return MakeCommand
                    .Do(() =>
                    {
                        var target = WorldViewModel.WorldNavigation.MouseOverEntity;
                        if (target.HasValue)
                        {
                            UnFollow();
                            _followEntities.Add(target.Value);
                            WorldViewModel.WorldNavigation.SetSelectedEntity(target.Value);
                        }
                        else
                            _followEntities = new HashSet<int>(WorldViewModel.WorldNavigation.SelectedEntities);
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
                _heading = NormalizeHeading(value);
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
        public EnumMobileState MobileState;

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
            Home.Execute(null);
            _movementTimer = new Stopwatch();
            _movementTimer.Start();
        }

        public bool IsPrimary { get { return WorldViewModel.CurrentPerspective == this; } }

        double _landSpeed = 50.0;
        DateTime _lastPositionSent;

        public void Check()
        {
            double deltaT = NextFrameDuration();

            Vector3D initialPosition = Position;
            Quaternion initialRotation = Rotation;

            Follow(deltaT);

            if (_actionState == InputBindings.ActionState.CreateAction)
                ApplyCreationActions();
            else
            {
                ApplyKeyBindings(deltaT);

                // Send update if required
                // TODO: use a global now, and use a dirty flag
                if ((Position != initialPosition || Rotation != initialRotation)
                    && (DateTime.Now - _lastPositionSent).TotalSeconds > 0.1)
                {
                    WorldViewModel.ServerConnection.MyPosition(
                        PossessingId, Position, Rotation, MobileState);
                    _lastPositionSent = DateTime.Now;
                }
            }
        }

        private InputBindings.ActionState _actionState = InputBindings.ActionState.KeyAction;

        private void ApplyKeyBindings(double deltaT)
        {
            int movementPerpendicular = 0;
            int movementForward = 0;
            int movementUp = 0;
            double speedModifier = 1;
            foreach (InputBindings.KeyBinding kb in WorldViewModel.InputBindings.KeyBindings
                .Where(kb => kb.KeyCombo.All(k => _keyPressed(k))))
            {
                UnFollow();

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
                    Tilt += deltaT * (AngleRangeHigh - AngleRangeLow) / 2.0;
                else if (kb.Action == InputBindings.KeyAction.TiltDown)
                    Tilt -= deltaT * (AngleRangeHigh - AngleRangeLow) / 2.0;
                else if (kb.Action == InputBindings.KeyAction.Walk)
                    speedModifier = 0.2;
                else if (kb.Action == InputBindings.KeyAction.Forward)
                    movementForward++;
                else if (kb.Action == InputBindings.KeyAction.Back)
                    movementForward--;
                else if (kb.Action == InputBindings.KeyAction.Home)
                    Home.Execute(null);
                else if (kb.Action == InputBindings.KeyAction.FollowSelected)
                    FollowSelected.Execute(null);

                else if (kb.Action == InputBindings.KeyAction.Possess)
                    PossessEntity.Execute(null);
                else if (kb.Action == InputBindings.KeyAction.Create)
                    _actionState = InputBindings.ActionState.CreateAction;

                else
                    throw new Exception("Unexpected keyboard binding " + kb.Action);
            }

            // Set Position and Rotation
            Rotation = new Quaternion(ZAxis, Heading) * new Quaternion(YAxis, -Tilt);

            if (movementPerpendicular == 0 && movementForward == 0 && movementUp == 0)
            {
                MobileState = EnumMobileState.Standing;
                return;
            }
            if (speedModifier < 1)
                MobileState = EnumMobileState.Walking;
            else
                MobileState = EnumMobileState.Running;

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

        private void ApplyCreationActions()
        {
            foreach (InputBindings.CreationBinding ca in WorldViewModel.InputBindings.CreationBindings
                .Where(ca => ca.KeyCombo.All(k => _keyPressed(k))))
            {
                _actionState = InputBindings.ActionState.KeyAction;
                if (ca.Action == InputBindings.CreationAction.Item)
                    CreateEntity.Execute(null);
                else if (ca.Action == InputBindings.CreationAction.Mobile)
                    CreateMobile.Execute(null);
                else if (ca.Action == InputBindings.CreationAction.Factory)
                    CreateFactory.Execute(null);
                else if (ca.Action == InputBindings.CreationAction.Mission)
                    CreateMission.Execute(null);
                else
                    throw new Exception("Unexpected creation binding " + ca.Action);
            }
        }

        static double NormalizeHeading(double degrees)
        {
            degrees = degrees % 360;
            return degrees < 0 ? degrees + 360 : degrees;
        }

        /// <summary> Move toward or follow one or more entities </summary>
        private void Follow(double deltaT)
        {
            // check they still exist in the world and get their current details
            var following = _followEntities
                .Select(WorldViewModel.History.Current.Entities.TryFind)
                .Where(x => x != null)
                .Select(x => x.Value);

            if (following.Any())
            {
                Vector3D center = following.Average(e => e.Position);
                Vector3D diff = center - Position;
                double vectorDistance = diff.Length;

                // TODO: replace with a proper bounds and frustum calculation
                var maxX = following.Max(e => e.Position.X);
                var maxY = following.Max(e => e.Position.Y);
                var maxZ = following.Max(e => e.Position.Z);
                var minX = following.Min(e => e.Position.X);
                var minY = following.Min(e => e.Position.Y);
                var minZ = following.Min(e => e.Position.Z);
                var viewDistance = new[] { 10.0, maxX - minX, maxY - minY, maxZ - minZ }.Max();

                // Move toward followed
                Vector3D target = center - (diff * viewDistance / vectorDistance);
                var move = target - Position;
                if (move.Length > 0.1)
                    Position += move * deltaT * 2;

                // Turn heading toward followed
                double idealHeading = Math.Atan2(diff.Y, diff.X)
                    * 180 / Math.PI;
                double turnLeft = NormalizeHeading(idealHeading - Heading);
                if (turnLeft >= 1)
                {
                    if (turnLeft <= 180)
                        Heading += 1;
                    else
                        Heading -= 1;
                }

                // Tilt toward followed
                double idealTilt = Math.Atan2(
                    diff.Z, Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y))
                    * 180 / Math.PI;
                double tiltUp = NormalizeHeading(idealTilt - Tilt);
                if (tiltUp >= 1)
                {
                    if (tiltUp <= 180)
                        Tilt += 0.5;
                    else
                        Tilt -= 0.5;
                }
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
    }
}
