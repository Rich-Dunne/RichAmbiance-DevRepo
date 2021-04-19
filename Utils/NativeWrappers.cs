using Rage;
using Rage.Native;
using RichAmbiance.Vehicles;

namespace RichAmbiance.Utils
{
    internal static class NativeWrappers
    {
        internal static void DrawMarker(int type, Vector3 position, float directionX, float directionY, float directionZ, float rotationX, float rotationY, float rotationZ, float scaleX, float scaleY, float scaleZ, int colorR, int colorG, int colorB, int alpha, bool bobUpAndDown, bool faceCamera, bool rotate, bool drawOnEntities)
        {
            NativeFunction.Natives.DRAW_MARKER(type, position, directionX, directionY, directionZ, rotationX, rotationY, rotationZ, scaleX, scaleY, scaleZ, colorR, colorG, colorB, alpha, bobUpAndDown, faceCamera, 2, rotate, 0, 0, drawOnEntities);
        }

        internal static bool GetRoadsidePoint(Vector3 position, out Vector3 roadSidePoint)
        {
            bool getRoadSidePoint = NativeFunction.Natives.x16F46FB18C8009E4<bool>(position, 3, out Vector3 roadSidePointValue);
            roadSidePoint = roadSidePointValue;
            return getRoadSidePoint;
        }

        internal static bool GetRoadsidePointWithHeading(Vector3 position, float heading, out Vector3 roadSidePoint)
        {
            bool getRoadSidePointWithHeading = NativeFunction.Natives.xA0F8A7517A273C05<bool>(position, heading, out Vector3 roadSidePointWithHeading);
            roadSidePoint = roadSidePointWithHeading;
            return getRoadSidePointWithHeading;
        }

        internal static bool GetClosestRoadNodeWithHeading(Vector3 position, out Vector3 closestPointOnRoad, out float closestPointOnRoadHeading)
        {
            bool getClosestPointOnRoadWithHeading = NativeFunction.Natives.xFF071FB798B803B0<bool>(position, out Vector3 roadPosition, out float heading, 1, 3f, 0f);
            closestPointOnRoad = roadPosition;
            closestPointOnRoadHeading = heading;
            return getClosestPointOnRoadWithHeading;
        }

        internal static bool GetNthClosestRoadNodeFavorDirection(Vector3 position, Vector3 desiredPosition, int nthClosest, out Vector3 nthClosestPointOnRoad, out float nthClosestPointHeading)
        {
            bool getNthClosestPoint = NativeFunction.Natives.x45905BE8654AE067<bool>(position, desiredPosition, nthClosest, out Vector3 roadPosition, out float heading, 1, 3f, 0);
            nthClosestPointOnRoad = roadPosition;
            nthClosestPointHeading = heading;
            return getNthClosestPoint;
        }

        internal static bool GetRoadNodeProperties(Vector3 position, out int density, out int flags)
        {
            bool getVehicleNodeProperties = NativeFunction.Natives.x0568566ACBB5DEDC<bool>(position, out int densityValue, out int flagsValue);
            density = densityValue;
            flags = flagsValue;
            return getVehicleNodeProperties;
        }

        internal static bool IsEntityPlayingAnimation(Entity entity, string animationDictionary, string animationName) => NativeFunction.Natives.IS_ENTITY_PLAYING_ANIM<bool>(entity, animationDictionary, animationName, 3);

        internal static void TaskStartScenarioInPlace(Ped ped, string scenarioName, int unkDelay = 0, bool playEnterAnimation = true) => NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(ped, scenarioName, 0, playEnterAnimation);

        internal static void StartVehicleHorn(Vehicle vehicle, int duration, string mode, bool forever) => NativeFunction.Natives.START_VEHICLE_HORN(vehicle, duration, mode, forever);

        internal static void TaskTurnPedToFaceEntity(Ped ped, Entity entity, int duration) => NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, entity, duration);

        internal static void TaskVehicleShootAtPed(Ped ped, Ped target, float unknown = 0) => NativeFunction.Natives.x10AB107B887214D8(ped, target, unknown);

        internal static void SetEntityMaxSpeed(Entity entity, float speed) => NativeFunction.Natives.SET_ENTITY_MAX_SPEED(entity, speed);

        internal static void SetVehicleLights(Vehicle vehicle, VehicleLightsState state) => NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, (int)state);

        internal static void SetVehicleBrakeLights(Vehicle vehicle, bool enabled) => NativeFunction.Natives.SET_VEHICLE_BRAKE_LIGHTS(vehicle, enabled);

        internal static void SetVehicleDamage(Vehicle vehicle, Vector3 position, float damage, float radius, bool focusOnModel) => NativeFunction.Natives.SET_VEHICLE_DAMAGE(vehicle, position, damage, radius, focusOnModel);

        internal static void CopyVehicleDamages(Vehicle vehicle, Vehicle targetVehicle) => NativeFunction.Natives.xE44A982368A4AF23(vehicle, targetVehicle);

        internal static void SmashVehicleWindow(Vehicle vehicle, int index) => NativeFunction.Natives.SMASH_VEHICLE_WINDOW(vehicle, index);

        internal static void SetDriveTaskCruiseSpeed(Ped driver, float cruiseSpeed) => NativeFunction.Natives.SET_DRIVE_TASK_CRUISE_SPEED(driver, cruiseSpeed);

        internal static void SetVehicleForwardSpeed(Vehicle vehicle, float speed) => NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(vehicle, speed);
    }
}
