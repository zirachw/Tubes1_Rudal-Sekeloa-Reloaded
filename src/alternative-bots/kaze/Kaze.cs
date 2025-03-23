// Modules and APIs
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
    private readonly double margin = 50;
    private double nearWallDistance;
    private double innerDistance;
    private bool nearWall = true; // true = near wall, false = inner.
    private int segmentCount = 0;

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
            // Note, pattern ini memungkinkan buat bot bergerak pseudo-random

            // Square pattern
            double currentDistance = nearWall ? nearWallDistance : innerDistance;
            Forward(currentDistance);
            TurnRight(90);
            segmentCount++;

            // Full rotation (4 segments)
            if (segmentCount >= 4)
            {
                if (nearWall)
                {
                    // Kalau selesai 1 rotasi, masuk lebih dalam ke arena
                    TurnRight(90);
                    Forward(margin);
                    nearWall = false;
                }
                else
                {
                    // Balik ke deket wall kalau udah selesai 1 rotasi lagi
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

        //    - Kalau deket banget (distance < 100), high-power bullet (type 3).
        //    - Kalau cukup deket (distance < 200), medium-power bullet (type 2).
        //    - Kalau jauh, low-power bullet (type 1).
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


        // Kalau ada energy advantage, coba lebih agresif
        if (Energy > e.Energy + 30)
        {
            bulletPower = Math.Min(3, Energy);
        }
        else if (e.Energy < 15)
        {
            // Coba buat ks
            bulletPower = 1;
        }

        SetFire(bulletPower);
        Rescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Kalau kena bot, manuver, dan coba untuk cari musuhnya
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

    // Menghitung rotasi bot biar bisa detect musuhnya (misal encounter)
    private double BearingTo(double targetX, double targetY)
    {
        double dx = targetX - X;
        double dy = targetY - Y;
        double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
        double bearing = angle - Direction;
        return NormalizeAngle(bearing);
    }

    // Normalize angle to [-180, 180] degrees. (Bakal dipakai buat scan)
    private double NormalizeAngle(double angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }
}
