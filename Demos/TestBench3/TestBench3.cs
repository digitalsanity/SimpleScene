﻿using System;
using SimpleScene;
using SimpleScene.Demos;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

namespace TestBench3
{
    public class TestBench3 : TestBenchBootstrap
    {
        protected SSScene missileParticlesScene;
        protected SSpaceMissilesVisualSimulationManager missileManager;

        protected SSObjectMesh attackerDrone;
        protected SSObjectMesh targetDrone;


        public TestBench3 ()
            : base("TestBench3: Missiles")
        {
            shadowmapDebugQuad.renderState.visible = false;
        }

        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (var game = new TestBench3()) {
                game.Run(30.0);
            }
        }

        protected override void setupScene ()
        {
            base.setupScene();

            missileParticlesScene = new SSScene (mainShader, pssmShader, instancingShader, instancingPssmShader);

            var droneMesh = SSAssetManager.GetInstance<SSMesh_wfOBJ> ("./drone2/", "Drone2.obj");
            //var droneMesh = SSAssetManager.GetInstance<SSMesh_wfOBJ> ("missiles", "missile.obj");

            // add drones
            attackerDrone = new SSObjectMesh (droneMesh);
            attackerDrone.Pos = new OpenTK.Vector3(-20f, 0f, -15f);
            attackerDrone.Orient(Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI/2f));
            //attackerDrone.Orient(Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI));
            attackerDrone.AmbientMatColor = new Color4(0.1f,0.1f,0.1f,0.1f);
            attackerDrone.DiffuseMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            attackerDrone.SpecularMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            attackerDrone.EmissionMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            //droneObj1.renderState.visible = false;
            attackerDrone.Name = "attacker drone";
            scene.AddObject (attackerDrone);

            targetDrone = new SSObjectMesh (droneMesh);
            targetDrone.Pos = new OpenTK.Vector3(20f, 0f, -15f);
            targetDrone.AmbientMatColor = new Color4(0.1f,0.1f,0.1f,0.1f);
            targetDrone.DiffuseMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            targetDrone.SpecularMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            targetDrone.EmissionMatColor = new Color4(0.3f,0.3f,0.3f,0.3f);
            targetDrone.Name = "target drone";
            targetDrone.MainColor = new Color4(1f, 0f, 0.7f, 1f);
            //droneObj2.renderState.visible = false;
            scene.AddObject (targetDrone);

            // manages missiles
            missileManager = new SSpaceMissilesVisualSimulationManager(scene, missileParticlesScene);
        }

        protected void missileKeyUpHandler(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Q) {
                _launchMissiles();
            } else if (e.Key == Key.M) {
                var camera = (scene.ActiveCamera as SSCameraThirdPerson);
                if (camera != null) {
                    var target = camera.FollowTarget;
                    if (target == null) {
                        camera.FollowTarget = attackerDrone;
                    } else if (target == attackerDrone) {
                        camera.FollowTarget = targetDrone;
                    } else {
                        camera.FollowTarget = null;
                    }
                    updateTextDisplay();
                }
            }
        }

        protected void _launchMissiles()
        {
            // TODO
        }

        protected override void updateTextDisplay ()
        {
            base.updateTextDisplay ();
            textDisplay.Label += "\n\nPress Q to fire missiles";

            var camera = scene.ActiveCamera as SSCameraThirdPerson;
            if (camera != null) {
                var target = camera.FollowTarget;
                textDisplay.Label += 
                    "\n\nPress M to toggle camera target: ["
                    + (target == null ? "none" : target.Name) + ']';
            }

        }

        protected override void setupInput()
        {
            base.setupInput();
            this.KeyUp += missileKeyUpHandler;
        }

        protected override void setupCamera()
        {
            var camera = new SSCameraThirdPerson (targetDrone);
            //var camera = new SSCameraThirdPerson (droneObj1);
            camera.Pos = Vector3.Zero;
            camera.followDistance = 80.0f;

            scene.ActiveCamera = camera;
            scene.AddObject (camera);
        } 
    }
}
