using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class GreedyTerminator : Bot
{
    private double scannedX;
    private double scannedY;
    private double enemyEnergy = 100;
    private int moveDirection = 1;
    private int lastScanTurn;
    
    static void Main(string[] args) => new GreedyTerminator().Start();

    GreedyTerminator() : base(BotInfo.FromFile("GeorgeDroyd.json")) { }
    
    public override void Run()
    {
        BodyColor = Color.Gold;
        TurretColor = Color.Black;
        RadarColor = Color.Red;

        while (IsRunning)
        {
            AdvancedMovement();
            RadarSweep();
        }
    }

    private void AdvancedMovement()
    {
        TurnLeft(45 * moveDirection);
        while (IsRunning && IsAiming())
        {
            Go(); 
        }
        
        double distance = 150 + new Random().Next(50);
        Forward(distance * moveDirection);
        moveDirection *= -1;
    }

    private void RadarSweep()
    {
        if (TurnNumber - lastScanTurn > 10)
            TurnRadarRight(360);
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        lastScanTurn = TurnNumber;
        
        double bearingRadians = ToRadians(GetBearingTo(evt.X, evt.Y));
        double distance = GetDistanceTo(evt.X, evt.Y);
        
        scannedX = evt.X;
        scannedY = evt.Y;
        
        TurnToFaceTarget(evt.X, evt.Y);
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        double evasionAngle = Direction + 90;
        Evade(evasionAngle);
        Back(50);
    }

    private void Evade(double bearing)
    {
        moveDirection *= -1;
        Forward(150);
    }

    private double NormalizeAngle(double angle)
    {
        angle %= (2 * Math.PI);
        return angle > Math.PI ? angle - (2 * Math.PI) : angle;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;
    private double ToDegrees(double radians) => radians * 180 / Math.PI;
    
    private double GetBearingTo(double x, double y)
    {
        double dx = x - X;
        double dy = y - Y;
        double angle = ToDegrees(Math.Atan2(dx, dy));
        return NormalizeAngleDegrees(angle - Direction);
    }
    
    private double GetDistanceTo(double x, double y)
    {
        double dx = x - X;
        double dy = y - Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    private double NormalizeAngleDegrees(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
    
    private bool IsTurning() => TurnRemaining != 0;
    private bool IsGunTurning() => GunTurnRemaining != 0;
    private bool IsRadarTurning() => RadarTurnRemaining != 0;
    
    private bool IsAiming() => IsTurning() || IsGunTurning() || IsRadarTurning();
}
