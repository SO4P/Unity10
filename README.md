# 网络版坦克大战
+ 使用上次作业的资源Tanks! Tutorial，稍微修改了这个项目的源代码。
+ 类似老师上课的课件，玩家，NetworkManager等设置基本相同：
    + NetworkManager

    ![0](https://github.com/SO4P/Unity10/blob/master/NetworkManager.PNG)
    + 玩家

    ![1](https://github.com/SO4P/Unity10/blob/master/Tank.PNG)
    + 子弹

    ![2](https://github.com/SO4P/Unity10/blob/master/Bullet.PNG)
    + 坦克爆炸
    
    ![3](https://github.com/SO4P/Unity10/blob/master/TankEx.PNG)
+ 尚未解决问题：
    客户端可能会出现卡顿，玩家的转向滞后，子弹射出经常丢失爆炸效果，而在host中正常
+ 部分代码：
    + 坦克移动
    ```
    using UnityEngine;
    using UnityEngine.Networking;

    namespace Complete
    {
        public class TankMovement : NetworkBehaviour
        {
            public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
            public float m_Speed = 12f;                 // How fast the tank moves forward and back.
            public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
            public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
            public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
            public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		    public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

            private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
            private string m_TurnAxisName;              // The name of the input axis for turning.
            private Rigidbody m_Rigidbody;              // Reference used to move the tank.
            private float m_MovementInputValue;         // The current value of the movement input.
            private float m_TurnInputValue;             // The current value of the turn input.
            private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
            private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

            private void Awake ()
            {
                m_Rigidbody = GetComponent<Rigidbody> ();
            }


            private void OnEnable ()
            {
                // When the tank is turned on, make sure it's not kinematic.
                m_Rigidbody.isKinematic = false;

                // Also reset the input values.
                m_MovementInputValue = 0f;
                m_TurnInputValue = 0f;

                // We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
                // It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
                // it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
                m_particleSystems = GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < m_particleSystems.Length; ++i)
                {
                    m_particleSystems[i].Play();
                }
            }


            private void OnDisable ()
            {
                // When the tank is turned off, set it to kinematic so it stops moving.
                m_Rigidbody.isKinematic = true;

                // Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
                for(int i = 0; i < m_particleSystems.Length; ++i)
                {
                    m_particleSystems[i].Stop();
                }
            }


            private void Start ()
            {

                // The axes names are based on player number.
                m_MovementAxisName = "Vertical" + m_PlayerNumber;
                m_TurnAxisName = "Horizontal" + m_PlayerNumber;

                // Store the original pitch of the audio source.
                m_OriginalPitch = m_MovementAudio.pitch;
            }


            private void Update ()
            {
                if (!isLocalPlayer)
                    return;
                // Store the value of both input axes.
                m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
                m_TurnInputValue = Input.GetAxis (m_TurnAxisName);

                EngineAudio ();
                Vector3 loc = this.transform.position;
                loc.y += 2;
                Camera.main.transform.position = loc;
                Camera.main.transform.rotation = this.transform.rotation;
            }

            public override void OnStartLocalPlayer()
            {
                this.BroadcastMessage("changeColor");
            }

            private void EngineAudio ()
            {
                // If there is no input (the tank is stationary)...
                if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
                {
                    // ... and if the audio source is currently playing the driving clip...
                    if (m_MovementAudio.clip == m_EngineDriving)
                    {
                        // ... change the clip to idling and play it.
                        m_MovementAudio.clip = m_EngineIdling;
                        m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                        m_MovementAudio.Play ();
                    }
                }
                else
                {
                    // Otherwise if the tank is moving and if the idling clip is currently playing...
                    if (m_MovementAudio.clip == m_EngineIdling)
                    {
                        // ... change the clip to driving and play.
                        m_MovementAudio.clip = m_EngineDriving;
                        m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                        m_MovementAudio.Play();
                    }
                }
            }


            private void FixedUpdate ()
            {
                // Adjust the rigidbodies position and orientation in FixedUpdate.
                Move ();
                Turn ();
            }


            private void Move ()
            {
                // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
                Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

                // Apply this movement to the rigidbody's position.
                m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
            }


            private void Turn ()
            {
                // Determine the number of degrees to be turned based on the input, speed and time between frames.
                float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

                // Make this into a rotation in the y axis.
                Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

                // Apply this rotation to the rigidbody's rotation.
                m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
            }
        }
    }
    ```
    + 坦克射击
    ```
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    namespace Complete
    {
        public class TankShooting : NetworkBehaviour
        {
            public int m_PlayerNumber = 1;              // Used to identify the different players.
            public GameObject m_Shell;                   // Prefab of the shell.
            public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
            public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
            public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
            public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
            public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
            public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
            public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
            public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


            private string m_FireButton;                // The input axis that is used for launching shells.
            private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
            private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
            private bool m_Fired;                       // Whether or not the shell has been launched with this button press.


            private void OnEnable()
            {
                // When the tank is turned on, reset the launch force and the UI
                m_CurrentLaunchForce = m_MinLaunchForce;
                m_AimSlider.value = m_MinLaunchForce;
            }


            private void Start()
            {
                // The fire axis is based on the player number.
                m_FireButton = "Fire" + m_PlayerNumber;

                // The rate that the launch force charges up is the range of possible forces by the max charge time.
                m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
            }


            private void Update()
            {
                if (!isLocalPlayer)
                    return;
                // The slider should have a default value of the minimum launch force.
                m_AimSlider.value = m_MinLaunchForce;

                // If the max force has been exceeded and the shell hasn't yet been launched...
                if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
                {
                    // ... use the max force and launch the shell.
                    m_CurrentLaunchForce = m_MaxLaunchForce;
                    CmdFire();
                }
                // Otherwise, if the fire button has just started being pressed...
                else if (Input.GetButtonDown(m_FireButton))
                {
                    // ... reset the fired flag and reset the launch force.
                    m_Fired = false;
                    m_CurrentLaunchForce = m_MinLaunchForce;

                    // Change the clip to the charging clip and start it playing.
                    m_ShootingAudio.clip = m_ChargingClip;
                    m_ShootingAudio.Play();
                }
                // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
                else if (Input.GetButton(m_FireButton) && !m_Fired)
                {
                    // Increment the launch force and update the slider.
                    m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                    m_AimSlider.value = m_CurrentLaunchForce;
                }
                // Otherwise, if the fire button is released and the shell hasn't been launched yet...
                else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
                {
                    // ... launch the shell.
                    CmdFire();
                }
            }

            [Command]
            void CmdFire()
            {
                // Set the fired flag so only Fire is only called once.
                m_Fired = true;

                // Create an instance of the shell and store a reference to it's rigidbody.
                GameObject shellInstance = (GameObject)Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);

                // Set the shell's velocity to the launch force in the fire position's forward direction.
                shellInstance.GetComponent<Rigidbody>().velocity = m_CurrentLaunchForce * m_FireTransform.forward;

                NetworkServer.Spawn(shellInstance);

                // Change the clip to the firing clip and play it.
                m_ShootingAudio.clip = m_FireClip;
                m_ShootingAudio.Play();

                // Reset the launch force.  This is a precaution in case of missing button events.
                m_CurrentLaunchForce = m_MinLaunchForce;
            }
        }
    }
    ```
    + 坦克血量
    ```
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    namespace Complete
    {
        public class TankHealth : NetworkBehaviour
        {
            public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
            public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
            public Image m_FillImage;                           // The image component of the slider.
            public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
            public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
            public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.
        
        
            private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
            private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.

            [SyncVar]
            private float m_CurrentHealth;                      // How much health the tank currently has.
            private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?


            private void Awake ()
            {
                // Instantiate the explosion prefab and get a reference to the particle system on it.
                m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

                // Get a reference to the audio source on the instantiated prefab.
                m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

                // Disable the prefab so it can be activated when it's required.
                m_ExplosionParticles.gameObject.SetActive (false);
            }


            private void OnEnable()
            {
                // When the tank is enabled, reset the tank's health and whether or not it's dead.
                m_CurrentHealth = m_StartingHealth;
                m_Dead = false;

                // Update the health slider's value and color.
                SetHealthUI();
            }

            private void FixedUpdate()
            {
                // Change the UI elements appropriately.
                SetHealthUI();
                // If the current health is at or below zero and it has not yet been registered, call OnDeath.
                if (m_CurrentHealth <= 0f && !m_Dead)
                {
                    OnDeath();
                }
            }

            public void TakeDamage (float amount)
            {
                if (!isServer)
                    return;
                // Reduce current health by the amount of damage done.
                m_CurrentHealth -= amount;

            }


            private void SetHealthUI ()
            {
                // Set the slider's value appropriately.
                m_Slider.value = m_CurrentHealth;

                // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
                m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
            }


            private void OnDeath ()
            {
                // Set the flag so that this function is only called once.
                m_Dead = true;

                // Move the instantiated explosion prefab to the tank's position and turn it on.
                m_ExplosionParticles.transform.position = transform.position;
                m_ExplosionParticles.gameObject.SetActive (true);

                // Play the particle system of the tank exploding.
                m_ExplosionParticles.Play ();

                // Play the tank explosion sound effect.
                m_ExplosionAudio.Play();

                // Turn the tank off.
                gameObject.SetActive (false);
            }
        
            public float getHp()
            {
                return m_CurrentHealth;
            }
        }
    }
    ```
+ 演示视频
