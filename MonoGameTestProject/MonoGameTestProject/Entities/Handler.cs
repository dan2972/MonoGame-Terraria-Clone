using Microsoft.Xna.Framework.Graphics;
using MonoGameTestProject.Entities.Item;
using MonoGameTestProject.Map;
using MonoGameTestProject.Runner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities
{
    class Handler
    {

        public ArrayList characterList { get; set; }
        public ArrayList projectileList { get; set; }
        public ArrayList noTickObjectList { get; set; }
        public ArrayList particleList { get; set; }
        public ArrayList itemDropList { get; set; }

        private Camera camera;

        private GameMap gMap;

        public Handler(Camera camera, GameMap gMap)
        {
            this.camera = camera;
            this.gMap = gMap;
            characterList = new ArrayList();
            projectileList = new ArrayList();
            particleList = new ArrayList();
            itemDropList = new ArrayList();
        }

        public void render(SpriteBatch sb)
        {
            gMap.render(sb);
            foreach (GameObject obj in characterList)
            {
                obj.render(sb);
            }
            foreach (GenericItem obj in itemDropList)
            {
                obj.render(sb);
            }
            gMap.renderLight(sb);
            foreach (GameObject obj in particleList)
            {
                obj.render(sb);
            }
            foreach (GameObject obj in projectileList)
            {
                obj.render(sb);
            }
        }

        public void tick(float gTime)
        {
            for (int i = 0; i < characterList.Count; i++)
            {
                GameObject obj = (GameObject)characterList[i];
                obj.tick(gTime);
                if (obj.getType() == "PLAYER")
                    camera.follow(obj.x, obj.y, obj.sizeX, obj.sizeY, gTime);
            }
            for (int i = 0; i < projectileList.Count; i++)
            {
                GameObject obj = (GameObject)projectileList[i];
                obj.tick(gTime);
            }
            for (int i = 0; i < particleList.Count; i++)
            {
                GameObject obj = (GameObject)particleList[i];
                obj.tick(gTime);
            }
            for (int i = 0; i < itemDropList.Count; i++)
            {
                GenericItem obj = (GenericItem)itemDropList[i];
                obj.tick(gTime);
            }
            //System.Console.WriteLine("Handler size: {0}", characterList.Count + projectileList.Count + particleList.Count + noTickObjectList.Count);
        }

        public void init()
        {
            
        }

        public void addObject(GameObject obj, ObjectType.type type)
        {
            switch (type)
            {
                case ObjectType.type.projectiles:
                    projectileList.Add(obj);
                    break;
                case ObjectType.type.characters:
                    characterList.Add(obj);
                    break;
                case ObjectType.type.particles:
                    particleList.Add(obj);
                    break;
            }
        }

        public void addObject(GenericItem obj)
        {
            itemDropList.Add(obj);
        }

        public void removeObject(GameObject obj, ObjectType.type type)
        {
            switch (type)
            {
                case ObjectType.type.projectiles:
                    projectileList.Remove(obj);
                    break;
                case ObjectType.type.characters:
                    characterList.Remove(obj);
                    break;
                case ObjectType.type.particles:
                    particleList.Remove(obj);
                    break;
                default:
                    
                    break;
            }
        }

        public void removeObject(GenericItem obj)
        {
            itemDropList.Remove(obj);
        }
    }
}
