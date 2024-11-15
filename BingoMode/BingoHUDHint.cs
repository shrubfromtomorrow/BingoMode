using UnityEngine;
using RWCustom;

namespace BingoMode
{
    public class BingoHUDHint
    {
        public PhysicalObject followObject;
        public int origRoom;
        public FSprite sprite;
        public bool requestRemove;
        public float deathFade;
        public Vector2 objectPos;
        public Vector2 lastObjectPos;
        public Vector2 offset;
        public RoomCamera followCamera;

        public BingoHUDHint(PhysicalObject followObject, int origRoom, string elementName, Color color, Vector2 offset, RoomCamera followCamera, string shader = "")
        {
            this.followObject = followObject;
            this.origRoom = origRoom;
            this.offset = offset;
            this.followCamera = followCamera;
            sprite = new FSprite(elementName) { color = color };
            if (shader != "") sprite.shader = Custom.rainWorld.Shaders[shader];

            objectPos = followObject.firstChunk.pos;
            lastObjectPos = objectPos;
            deathFade = 1f;
        }

        public void Update()
        {
            if (requestRemove)
            {
                deathFade = Mathf.Max(0f, deathFade - 0.09f);
                return;
            }
            if (followObject == null || followObject.room == null || followObject.room.abstractRoom.index != followCamera.room.abstractRoom.index || origRoom == -1 || followObject.room.abstractRoom.index != origRoom || (followObject is Creature crit && crit.dead) || (sprite.element.name != "pipis" && followObject.grabbedBy.Count != 0))
            {
                requestRemove = true;
                return;
            }

            lastObjectPos = followObject.firstChunk.lastPos;
            objectPos = followObject.firstChunk.pos;
            if (sprite.element.name == "pipis") objectPos += Custom.RNV() * 2f * Random.value;
        }

        public void Draw(float timeStacker, Vector2 camPos)
        {
            sprite.SetPosition(Vector2.Lerp(lastObjectPos, objectPos, timeStacker) + offset - camPos);
            sprite.alpha = deathFade;
        }

        public void RemoveSprites()
        {
            sprite.RemoveFromContainer();
        }
    }
}
