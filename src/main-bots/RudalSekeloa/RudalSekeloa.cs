using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/* 
------------------------------------------------------------------
RudalSekeloa - A hybrid heuristic bot with main Tracking Scanner
------------------------------------------------------------------
Main Heuristics:
- Offensive mode: main tracking with strafing to an enemy
- Defensive mode: wavy movement along the walls

Combines:
- Kaze's energy-aware firing strategy
- Sweepredict's radar tracking and enemy prediction
- Waves's wavy defensive movement for low energy situations
- Waves's predictive targeting with confidence levels
------------------------------------------------------------------
*/

public class RudalSekeloa : Bot
{
    // Energy threshold for switching to defensive mode
    private const double lowEnergyThreshold = 30.0; 
    
    // Desired tracking distance from enemy 
    private const double trackingDistance = 150; 

    // Tolerance range for distance adjustment
    private const double tolerance = 20;         

    // Scanned bot information
    private ScannedBotEvent lastScannedBot = null;

    // Strafing direction (1 = right, -1 = left)
    private int strafeDirection = 1;
    
    // Size of the wave
    private const double WaveAmplitude = 60; 

    // Controls frequency of waves
    private const double WavePeriod = 40; 

    // Maximum moves to limit continuous waving
    private const int maxMoves = 50;

    // Padding borders
    private const double padding = 30;

    // Maximum allowed shooting distance
    private const double maxShootingDistance = 500;

    // Each side of padded area
    private double left, right, bottom, top;

    // Firepower
    private double firePower;

    // Bot state
    private bool isDefensiveMode = false;

    static void Main(string[] args)
    {
        new RudalSekeloa().Start();
    }

    RudalSekeloa() : base(BotInfo.FromFile("RudalSekeloa.json")) { }

    public override void Run()
    {
        // Set-up colors
        BodyColor = Color.DarkGreen;
        TurretColor = Color.DarkBlue;
        RadarColor = Color.Yellow;
        BulletColor = Color.Black;
        ScanColor = Color.Cyan;
        
        // Configure independent turning
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        
        // Define boundary limits for defensive mode
        left = padding;
        right = ArenaWidth - padding;
        bottom = padding;
        top = ArenaHeight - padding;
        
        while (IsRunning)
        {
            // Update defensive mode based on energy level
            isDefensiveMode = Energy < lowEnergyThreshold;

            // Continuous scanning
            SetTurnRadarRight(360);
            SetTurnRadarLeft(360);

            if (lastScannedBot == null)
            {
                // If no target, keep scanning
                SetTurnRadarRight(360);
                SetTurnRadarLeft(360);
            }
            
            // Defensive movement if energy is low
            if (isDefensiveMode)
            {
                if (lastScannedBot != null) TrackTargetRadarOnly();
                GoToNearestMiddleOfASide();
                WaitFor(new MovementCompleteCondition(this));
                MoveWavySide();
            }

            // Offensive tracking otherwise
            else
            {
                if (lastScannedBot != null) TrackTarget();
                else SetTurnRadarRight(360);
                Go();
            }
        }
    }
    
    // Track target with strafing and distance adjustment
    private void TrackTarget()
    {
        if (lastScannedBot == null) return;

        double enemyX = lastScannedBot.X;
        double enemyY = lastScannedBot.Y;
        double angleToEnemy = Math.Atan2(enemyY - Y, enemyX - X) * 180 / Math.PI;
        double distanceToEnemy = Math.Sqrt((enemyX - X) * (enemyX - X) + (enemyY - Y) * (enemyY - Y));

        // Aim gun at enemy
        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);

        // Adjust distance if not within the desired tracking range
        double distanceError = distanceToEnemy - trackingDistance;
        if (Math.Abs(distanceError) > tolerance)
        {
            if (distanceError > 0)
            {
                SetForward(Math.Min(distanceError, 100));
            }
            else
            {
                SetBack(Math.Min(-distanceError, 100));
            }
        }

        // Do strafing movement otherwise
        else
        {
            double desiredStrafeAngle = NormalizeAbsoluteAngle(angleToEnemy + 90 * strafeDirection);
            double turnToStrafe = NormalizeRelativeAngle(desiredStrafeAngle - Direction);

            SetTurnRight(turnToStrafe);
            SetForward(70);
        }
    }
    
    // Track only with radar and gun (for defensive mode)
    private void TrackTargetRadarOnly()
    {
        if (lastScannedBot == null) return;

        double enemyX = lastScannedBot.X;
        double enemyY = lastScannedBot.Y;
        double angleToEnemy = Math.Atan2(enemyY - Y, enemyX - X) * 180 / Math.PI;

        // Aim gun at enemy
        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);
        
        // Lock radar on enemy with wider sweep (overcorrection)
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        SetTurnRadarLeft(radarTurn * 2);
    }
    
    // Method to move to the nearest middle of a side of the padded area
    private void GoToNearestMiddleOfASide()
    {
        double middleX = (left + right) / 2;
        double middleY = (bottom + top) / 2;

        // Find the distance to the middle point of each side using the built-in DistanceTo method
        double dLeftSide = DistanceTo(left, middleY);
        double dRightSide = DistanceTo(right, middleY);
        double dBottomSide = DistanceTo(middleX, bottom);
        double dTopSide = DistanceTo(middleX, top);
        
        // Determine which side middle is closest
        double minDist = dLeftSide;
        string nearestSide = "left";
        if (dRightSide < minDist) 
        { 
            minDist = dRightSide; 
            nearestSide = "right"; 
        }

        if (dBottomSide < minDist) 
        { 
            minDist = dBottomSide; 
            nearestSide = "bottom"; 
        }

        if (dTopSide < minDist) 
        { 
            minDist = dTopSide; 
            nearestSide = "top"; 
        }
        
        // Check if already at the middle of a side (distance is very close to 0)
        bool alreadyAtMiddle = minDist < 0.001;
        
        if (nearestSide == "left")
        {
            // Turn towards middle of left side and facing northeast
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(middleY - Y, left - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(45);
        }
        else if (nearestSide == "right")
        {
            // Turn towards middle of right side and facing southwest
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(middleY - Y, right - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(225);
        }
        else if (nearestSide == "bottom")
        {
            // Turn towards middle of bottom side and facing northwest
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(bottom - Y, middleX - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(135);
        }
        else // (nearestSide == "top")
        {
            
            // Turn towards middle of top side and facing southeast
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(top - Y, middleX - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }
            TurnToCardinalDirection(315);
        }
    }
    
    // Helper method to turn to an exact cardinal direction
    private void TurnToCardinalDirection(double targetDirection)
    {
        double turnAmount = NormalizeRelativeAngle(targetDirection - Direction);
        TurnLeft(turnAmount);
        WaitFor(new TurnCompleteCondition(this));
    }

    // Move along one side of the wavy square
    private void MoveWavySide()
    {
        // First check if we're out of bounds and handle it efficiently
        if (X < left || X > right || Y < bottom || Y > top)
        {
            HandleOutOfBounds();
            return;
        }

        // Keep track of current wave position
        double wavePosition = 0;
        double stepSize = 5;
        int moveCounter = 0;
        
        double distanceX = X - left;
        double distanceY = Y - bottom;
        
        while (distanceX > -3 && 
                distanceY > -3 && 
                distanceX < right - left + 3 && 
                distanceY < top - bottom + 3 && 
                moveCounter < maxMoves)
        {
            // Calculate the wave angle
            double waveAngle = Math.Sin(wavePosition / WavePeriod) * WaveAmplitude;
            
            // Turn to follow the wave pattern
            SetTurnRight(waveAngle);
            SetForward(stepSize);
            Go();

            distanceX = X - left;
            distanceY = Y - bottom;

            wavePosition += stepSize;
            moveCounter++;
        }
    }
    
    // Handle out of bounds situations efficiently
    private void HandleOutOfBounds()
    {
        // Stop any current movement
        Stop();
        
        double targetDirection = -1;
        
        // Determine which boundary we've crossed
        if      (X < left && Y < bottom) targetDirection = 45;      // Northeast
        else if (X < left && Y > top) targetDirection = 315;        // Southeast
        else if (X > right && Y < bottom) targetDirection = 135;    // Northwest
        else if (X > right && Y > top) targetDirection = 225;       // Southwest
        else if (X < left) targetDirection = 0;                     // East
        else if (X > right) targetDirection = 180;                  // West
        else if (Y < bottom) targetDirection = 90;                  // North
        else if (Y > top) targetDirection = 270;                    // South
        
        if (targetDirection != -1)
        {
            double turnAmount = NormalizeRelativeAngle(targetDirection - Direction);
            
            SetTurnLeft(turnAmount);
            Go();
            WaitFor(new TurnCompleteCondition(this));
            
            double moveDistance = 40;
            SetForward(moveDistance);
            Go();
            WaitFor(new MovementCompleteCondition(this));
        }
    }

    // Handling scanned bots with hybrid targeting strategy
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Select target greedily: track enemy with lowest energy or current target.
        if (lastScannedBot == null || e.ScannedBotId == lastScannedBot.ScannedBotId || e.Energy < lastScannedBot.Energy)
        {
            lastScannedBot = e;
        }
        
        double distance = DistanceTo(e.X, e.Y);
        double angleToEnemy = Math.Atan2(e.Y - Y, e.X - X) * 180 / Math.PI;
        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);

        // Lock radar on target with overcorrection
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        SetTurnRadarLeft(radarTurn * 2);

        if (distance > maxShootingDistance)
        {
            // Still track with the gun, but don't fire
            SetTurnGunLeft(gunTurn);
            return;
        }
    
        // Determine fire power based on factors
        firePower = calculateFirePower(distance);
        if (Energy > e.Energy + 30) firePower = Math.Min(firePower, Energy / 10);
        if (e.Energy < 16)
        {
            if (distance < 100) firePower = 3.0;
            else if (distance < 300) firePower = 2.5;
        };
        firePower = Math.Min(firePower, Energy);

        // Calculate bullet speed and time to hit
        double bulletSpeed = 20 - 3 * firePower;
        double timeToHit = distance / bulletSpeed;
        
        // Predict enemy's future position
        double enemyHeadingRadians = e.Direction * Math.PI / 180; 
        double predictedX = e.X + Math.Cos(enemyHeadingRadians) * e.Speed * timeToHit;
        double predictedY = e.Y + Math.Sin(enemyHeadingRadians) * e.Speed * timeToHit;
        
        // Calculate angle to the predicted position
        double angleToEnemyPredicted = Math.Atan2(predictedY - Y, predictedX - X) * 180 / Math.PI;
        double gunTurnPredicted = NormalizeRelativeAngle(angleToEnemyPredicted - GunDirection);
        SetTurnGunLeft(gunTurnPredicted);
        
        // Calculate aiming confidence
        double aimConfidence;

        // Very high confidence for close targets
        if (distance < 50) aimConfidence = 0.95; 
        else 
        {
            aimConfidence = 1.0;
            aimConfidence *= 1.0 - (distance / 1000.0);                                // Distance factor
            aimConfidence *= 1.0 - (Math.Min(8, e.Speed) / 16.0);                      // Speed factor
            aimConfidence *= 1.0 - (Math.Min(45, Math.Abs(gunTurnPredicted)) / 120.0); // Gun turn factor
        }
        
        // Determine maximum allowed gun angle error
        double maxGunAngleError = distance < 50 ? 25 : 15 * aimConfidence;
        
        // Fire if we're confident enough and gun is cool
        if (Math.Abs(gunTurnPredicted) < maxGunAngleError && (aimConfidence > 0.3) && GunHeat == 0)
        {
            // Close combat keeps full power, longer distances may reduce power slightly
            double adjustedFirePower = distance < 50 ? firePower : firePower * Math.Max(0.8, aimConfidence);
            Fire(adjustedFirePower);
        }
    }

    // Determine fire power based on factors
    public double calculateFirePower(double distance)
    {
        firePower = 1.0;                        

        if      (distance <= 50)  firePower = 3.0;      // Close range: maximum power
        else if (distance <= 150) firePower = 2.5;      // Medium-close: high power
        else if (distance <= 300) firePower = 2.0;      // Medium range: medium power
        else if (distance <= 450) firePower = 1.5;      // Medium-far: moderate power

        return firePower;
    }

    // Dodge when hit by a bullet
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Change strafe direction to dodge
        strafeDirection *= -1;
    }

    // Improved collision handling with other bots
    public override void OnHitBot(HitBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double bearing = BearingTo(e.X, e.Y);
        
        // Being rammed by another bot
        if (e.IsRammed) 
        {
            Stop();
            
            // Turn gun toward enemy and fire with power based on distance
            double gunTurn = NormalizeRelativeAngle(bearing - GunDirection);
            TurnGunLeft(gunTurn);
            
            // Fire with power based on distance
            firePower = calculateFirePower(distance);
            Fire(firePower);
        }

        // Rammed into another bot
        else 
        {
            Stop();
            
            double gunTurn = NormalizeRelativeAngle(bearing - GunDirection);
            TurnGunLeft(gunTurn);
            
            // Fire with power based on distance
            firePower = calculateFirePower(distance);
            Fire(firePower);
        }
    }

    // When we hit a wall, adjust course to recover
    public override void OnHitWall(HitWallEvent e)
    {
        if (isDefensiveMode)
        {
            Stop();
            
            bool hitLeftWall = X <= left + 5;
            bool hitRightWall = X >= right - 5;
            bool hitBottomWall = Y <= bottom + 5;
            bool hitTopWall = Y >= top - 5;
            
            double retreatDirection = Direction;
            
            if      (hitLeftWall) retreatDirection = 0;     // Move east
            else if (hitRightWall) retreatDirection = 180;  // Move west
            else if (hitBottomWall) retreatDirection = 90;  // Move north
            else if (hitTopWall) retreatDirection = 270;    // Move south
            
            double turnAmount = NormalizeRelativeAngle(retreatDirection - Direction);
            
            SetTurnLeft(turnAmount);
            Go();
            WaitFor(new TurnCompleteCondition(this));
            
            SetForward(50);
            Go();
            WaitFor(new MovementCompleteCondition(this));
            
            GoToNearestMiddleOfASide();
        }
        else
        {
            // Reverse the strafing direction upon hitting a wall to help avoid getting stuck
            strafeDirection *= -1;
            SetTurnRight(90);
        }
    }
}

// Condition that is triggered when the turning is complete
public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.TurnRemaining == 0;
}

// Condition that is triggered when the movement is complete
public class MovementCompleteCondition : Condition
{
    private readonly Bot bot;

    public MovementCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.DistanceRemaining == 0;
}