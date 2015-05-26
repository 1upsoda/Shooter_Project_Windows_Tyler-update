using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Shooter.View;
using Shooter.Model;

namespace Shooter.Controller
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShooterGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Player player;
        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // A movement speed for the player
        float playerMoveSpeedX;
        public static float playerMoveSpeedY;
        public static float PlayerMoveSpeedY
        {
            get { return playerMoveSpeedY; }
            set { playerMoveSpeedY = value; }
        }
        float maxX;
        float maxY;
        int positionIncrease;
        string sodaProgress;
        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;
        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;
        Texture2D projectileTexture;
        List<Projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime; 
        Texture2D explosionTexture;
        List<Animation> explosions;
        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;
        //Number that holds the player score
        int score;
        Random randomSpawnTime = new Random();
        // The font used to display UI elements
        SpriteFont font;
        public string currentWeapon;
        public static string currentStyle;
        public static string CurrentStyle
        {
            get { return currentStyle; }
            set { currentStyle = value; }
        }

        public ShooterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player = new Player();
            // Set a constant player move speed
            playerMoveSpeedX = 0.0f;
            playerMoveSpeedY = 0.0f;
            positionIncrease = 0;
            maxX = 10.0f;
            maxY = 10.0f;
            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();
            // Initialize the enemies list
            enemies = new List<Enemy>();
            sodaProgress = "";
            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();
            projectiles = new List<Projectile>();
            currentWeapon = "Regular";
            currentStyle = "Regular";
            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(0.25f);
            explosions = new List<Animation>();
            //Set player's score to zero
            score = 0;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
           
           /// Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            ///+ GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("Images/shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);
            // Load the parallaxing background
            bgLayer1.Initialize(Content, "Images/bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "Images/bgLayer2", GraphicsDevice.Viewport.Width, -2);

            mainBackground = Content.Load<Texture2D>("Images/mainbackground");
            enemyTexture = Content.Load<Texture2D>("Images/mineAnimation");
            projectileTexture = Content.Load<Texture2D>("Images/laser");
            explosionTexture = Content.Load<Texture2D>("Images/explosion");
            // Load the music
            gameplayMusic = Content.Load<Song>("Sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("Sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("Sound/explosion");

            // Start the music right away
            PlayMusic(gameplayMusic);
            // Load the score font
            font = Content.Load<SpriteFont>("Fonts/gameFont");
        }
        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }
        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            projectiles.Add(projectile);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }
        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                enemySpawnTime = TimeSpan.FromSeconds(randomSpawnTime.Next(1, 3));
                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);
                        // Play the explosion sound
                        explosionSound.Play();
                        //Add to the player's score
                        score += enemies[i].Value;
                    }
                    enemies.RemoveAt(i);
                }
                
            }
            
        }
        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                this.Exit();
            }
            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            

            //Update the player
            UpdatePlayer(gameTime);
            bgLayer1.Update();
            bgLayer2.Update();
            // Update the enemies
            UpdateEnemies(gameTime);
            UpdateCollision();
            UpdateProjectiles();
            UpdateExplosions(gameTime);
            UpdateCheats();
            base.Update(gameTime);
        }
        private void UpdateCheats()
        {
            if ((currentKeyboardState.IsKeyDown(Keys.D1) || currentKeyboardState.IsKeyDown(Keys.NumPad1)) && sodaProgress != "1upsoda")
            {
                sodaProgress = "1";
            }
            if (currentKeyboardState.IsKeyDown(Keys.U) && sodaProgress == "1")
            {
                sodaProgress = "1u";
            }
            if (currentKeyboardState.IsKeyDown(Keys.P) && sodaProgress == "1u")
            {
                sodaProgress = "1up";
            }
            if (currentKeyboardState.IsKeyDown(Keys.S) && sodaProgress == "1up")
            {
                sodaProgress = "1ups";
            }
            if (currentKeyboardState.IsKeyDown(Keys.O) && sodaProgress == "1ups")
            {
                sodaProgress = "1upso";
            }
            if (currentKeyboardState.IsKeyDown(Keys.D) && sodaProgress == "1upso")
            {
                sodaProgress = "1upsod";
            }
            if (currentKeyboardState.IsKeyDown(Keys.A) && sodaProgress == "1upsod")
            {
                sodaProgress = "1upsoda";
                currentWeapon = "OP LAZER";
                fireTime = TimeSpan.FromSeconds(0.00001f);
            }
            if (currentKeyboardState.IsKeyDown(Keys.L))
            {
                currentWeapon = "Lazer";
                sodaProgress = "";
                fireTime = TimeSpan.FromSeconds(0.001f);
            }
            if(currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                for(int i=0; i<50; i++)
                {
                    AddEnemy();
                }
            }
            if(currentKeyboardState.IsKeyDown(Keys.C))
            {
                currentStyle = "Drop";
                sodaProgress = "";
            }
            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                currentStyle = "Sin Wave";
                sodaProgress = "";
            }
            if (currentKeyboardState.IsKeyDown(Keys.X))
            {
                currentStyle = "Sin-ish";
                sodaProgress = "";
                Projectile.dropMove = 0;
                Projectile.sinUp = true;
            }
            if (currentKeyboardState.IsKeyDown(Keys.R))
            {
                currentStyle = "Regular";
                currentWeapon = "Regular";
                sodaProgress = "";
                fireTime = TimeSpan.FromSeconds(0.25f);
            }
        }
        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }
        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);
            
            // Get Thumbstick Controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeedX;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeedY;

            // Fire only every interval we set as the fireTime
            if (gameTime.TotalGameTime - previousFireTime > fireTime && currentKeyboardState.IsKeyDown(Keys.Space))
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;
                if (currentWeapon == "OP LAZER")
                {
                    // Add the projectile, but add it to the front and center of the player
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - 42));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - 84));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - 126));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - 168));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - 210));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - 210));
                    positionIncrease = 252;
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - positionIncrease));
                    positionIncrease = 294;
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - positionIncrease));
                    positionIncrease = 336;
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - positionIncrease));
                    positionIncrease = 378;
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 2 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 5 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 8 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 10 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 12 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 15 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 18 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 20 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 22 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 25 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 28 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 30 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 32 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 35 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 38 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 40 + positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - positionIncrease));
                    positionIncrease = 420;
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -0 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -2 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -5 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -8 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -10 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -12 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -15 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -18 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -20 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -22 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -25 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -28 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -30 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -32 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -35 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -38 - positionIncrease));
                    AddProjectile(player.Position + new Vector2(player.Width / 2, -40 - positionIncrease));
                    // Play the laser sound
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                    laserSound.Play();
                }
                else if (currentWeapon == "Regular")
                {
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    laserSound.Play();
                }
                else if (currentWeapon == "Lazer")
                {
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                    laserSound.Play();
                }
            }

            // Use the Keyboard / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.D) ||
            currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                if (playerMoveSpeedX < maxX)
                {
                    playerMoveSpeedX = (playerMoveSpeedX + 0.6f);
                }
                else
                {
                    playerMoveSpeedX = 10.0f;
                }
            }
            if (currentKeyboardState.IsKeyDown(Keys.A) ||
            currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                if (playerMoveSpeedX > -maxX)
                {
                    playerMoveSpeedX = (playerMoveSpeedX - 0.6f);
                }
                else
                {
                    playerMoveSpeedX = -10.0f;
                }
            }
            if (currentKeyboardState.IsKeyDown(Keys.S) ||
            currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                if (playerMoveSpeedY < maxY)
                {
                    playerMoveSpeedY = (playerMoveSpeedY + 0.6f);
                }
                else
                {
                    playerMoveSpeedY = 10.0f;
                }
            }
            if (currentKeyboardState.IsKeyDown(Keys.W) ||
            currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                if (playerMoveSpeedY > -maxY)
                {
                    playerMoveSpeedY = (playerMoveSpeedY - 0.6f);
                }
                else
                {
                    playerMoveSpeedY = -10.0f;
                }
            }
            if (playerMoveSpeedX > 0.00f && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D))
            {
                playerMoveSpeedX = (playerMoveSpeedX - 0.4f);
            }
            if (playerMoveSpeedX < 0.00f && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D))
            {
                playerMoveSpeedX = (playerMoveSpeedX + 0.4f);
            }
            if (playerMoveSpeedY > 0.00f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY - 0.4f);
            }
            if (playerMoveSpeedY < 0.00f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY + 0.4f);
            }
            if (playerMoveSpeedX > 0.0000f && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D))
            {
                playerMoveSpeedX = (playerMoveSpeedX - 0.01f);
            }
            if (playerMoveSpeedX < 0.00f && !currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.A))
            {
                playerMoveSpeedX = (playerMoveSpeedX + 0.01f);
            }
            if (playerMoveSpeedY > 0.00f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY - 0.01f);
            }
            if (playerMoveSpeedY < 0.00f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY + 0.01f);
            }
            if (playerMoveSpeedX > 0.0000000f && !currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.A) )
            {
                playerMoveSpeedX = (playerMoveSpeedX - 0.001f);
            }
            if (playerMoveSpeedX < 0.000000f && !currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.A))
            {
                playerMoveSpeedX = (playerMoveSpeedX + 0.001f);
            }
            if (playerMoveSpeedY > 0.000000f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY - 0.001f);
            }
            if (playerMoveSpeedY < 0.000000f && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
            {
                playerMoveSpeedY = (playerMoveSpeedY + 0.001f);
            }

            player.Position.X = (player.Position.X + playerMoveSpeedX);
            player.Position.Y = (player.Position.Y + playerMoveSpeedY);
            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);
            // reset score if player health goes to zero
            if (player.Health <= 0)
            {
                player.Health = 100;
                score = 0;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            // Draw the moving background
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);
            player.Draw(spriteBatch);
            
            // Draw the Enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(spriteBatch);
            }
            // TODO: Add your drawing code here
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(spriteBatch);
            }
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(spriteBatch);
                // Draw the score
            }            

            spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
            spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
            spriteBatch.DrawString(font, "weapon: " + currentWeapon, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 60), Color.White);
            spriteBatch.DrawString(font, "style: " + currentStyle, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 90), Color.White);

            if (player.Health < 30)
            {
                spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.Red);

            }
            spriteBatch.DrawString(font, "X: " + Projectile.newXpos, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 120), Color.White);
            spriteBatch.DrawString(font, "Y: " + Projectile.newYpos, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 150), Color.White);

            spriteBatch.End();
            
            base.Draw(gameTime);
        }
        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,
            (int)player.Position.Y,
            player.Width,
            player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,
                (int)enemies[i].Position.Y,
                enemies[i].Width,
                enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero we died
                    if (player.Health <= 0)
                        player.Active = false;
                }

            }
            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
                    (int)enemies[j].Position.Y - enemies[j].Height / 2,
                    enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        if (currentWeapon == "OP LAZER")
                        {
                            enemies[j].Health -= (projectiles[i].Damage * 8000);
                        }
                        else if (currentWeapon == "Lazer")
                        {
                            enemies[j].Health -= (projectiles[i].Damage);
                            projectiles[i].Active = false;
                        }
                        else
                        {
                            enemies[j].Health -= (projectiles[i].Damage*5);
                            projectiles[i].Active = false;
                        }
                    }
                }
            }
        }
    }
}
