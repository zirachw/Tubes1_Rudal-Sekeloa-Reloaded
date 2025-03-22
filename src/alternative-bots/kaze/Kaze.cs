// Module
using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/* 
------------------------------------------------------------------
Kaze
------------------------------------------------------------------
A bot that follows a square movement pattern along the arena walls.
It rotates near the walls and shifts inward and outward as it completes rotations.
In normal mode, it fires with different bullet types (small, medium, large) based on the enemy's distance and relative energy, improving its chances of winning
------------------------------------------------------------------
Kaze da~
*/

public class Kaze : Bot
{
    private readonly double margin = 50;           // Margin for shifting inward/outward.
    private double nearWallDistance;               // Side length when moving near the wall.
    private double innerDistance;                  // Side length when moving inside.
    private bool nearWall = true;                  // Current mode: true = near wall, false = inner.
    private int segmentCount = 0;                  // Counts segments in the current rotation.

    static void Main(string[] args)
    {
        new Kaze().Start();
    }

    Kaze() : base(BotInfo.FromFile("Kaze.json")) { }

    public override void Run()
    {
        // Set-up colors
        BodyColor = Color.DarkCyan;
        TurretColor = Color.DarkBlue;
        RadarColor = Color.Cyan;
        BulletColor = Color.White;
        ScanColor = Color.DarkRed;

        // Movement
        nearWallDistance = Math.Min(ArenaWidth, ArenaHeight) - margin;
        innerDistance = nearWallDistance - 2 * margin;

        // Buat bot di dekat arena wall
        TurnRight(Direction % 90);
        Forward(nearWallDistance);

        while (IsRunning)
        {
            // Square pattern
            double currentDistance = nearWall ? nearWallDistance : innerDistance;
            Forward(currentDistance);
            TurnRight(90);
            segmentCount++;

            // Full rotation (4 segments) completed.
            if (segmentCount >= 4)
            {
                if (nearWall)
                {
                    // Completed near-wall rotation: shift inward.
                    TurnRight(90);
                    Forward(margin);
                    nearWall = false;
                }
                else
                {
                    // Completed inner rotation: reposition back near the wall.
                    TurnLeft(90);
                    Forward(margin);
                    nearWall = true;
                }
                segmentCount = 0;
            }
            Rescan();
        }
    }

    // Musuh terlihat
    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double bulletPower = 1;

        //    - Kalau deket banget (< 100 jarak), high-power bullet (type 3).
        //    - Kalau cukup deket (< 200 jarak), use medium-power bullet (type 2).
        //    - Kalau jauh, low-power bullet.
        if (distance < 100)
        {
            bulletPower = 3;
        }
        else if (distance < 200)
        {
            bulletPower = 2;
        }
        else
        {
            bulletPower = 1;
        }

        // 2. Adjust based on relative energy:
        //    - If we have a significant energy advantage over the enemy, be aggressive.
        //    - If the enemy’s energy is low, conserve energy by reducing bullet power.
        // Note: e.Energy represents the enemy bot's energy (provided by the API).
        if (Energy > e.Energy + 30)
        {
            // If we are much stronger, try to finish the enemy quickly.
            bulletPower = Math.Min(3, Energy);
        }
        else if (e.Energy < 15)
        {
            // If the enemy is low on energy, use minimal firepower to conserve energy.
            bulletPower = 1;
        }

        SetFire(bulletPower);
        Rescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double bearing = BearingTo(e.X, e.Y);
        if (bearing > -90 && bearing < 90)
            Back(100);
        else
            Forward(100);
    }

    // Manuver (dodge kalau kena bullet)
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        TurnRight(45);
        Forward(50);
    }

    // Hitung jarak bot ke musuh
    private double DistanceTo(double targetX, double targetY)
    {
        double dx = targetX - X;
        double dy = targetY - Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // Helper: Calculate the bearing from current position to target.
    private double BearingTo(double targetX, double targetY)
    {
        double dx = targetX - X;
        double dy = targetY - Y;
        double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
        double bearing = angle - Direction;
        return NormalizeAngle(bearing);
    }

    // Normalize to [-180, 180] degrees.
    private double NormalizeAngle(double angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }
}
