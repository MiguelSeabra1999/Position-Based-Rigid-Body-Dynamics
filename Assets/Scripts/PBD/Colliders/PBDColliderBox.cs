using System;
using UnityEngine;

public class PBDColliderBox : PBDCollider
{
    private double EPSILON = 0.00001;
    public Vector3 dims = new Vector3(1, 1, 1);
    public DoubleVector3 halfDims;
    [HideInInspector] public double diagonal;
    private Color color;
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(pos, 0.2f);
    }

    void Awake()
    {
        color = GetComponent<Renderer>().material.color;

        halfDims = new DoubleVector3(transform.localScale) / 2;
        diagonal = DoubleVector3.Magnitude(halfDims);
    }

    public override void CalcBoundingBox()
    {
        DoubleVector3[] points = new DoubleVector3[8];
        GetVertices(ref points);
        double[] min = new double[] {points[0].x, points[0].y, points[0].z};
        double[] max = new double[] {points[0].x, points[0].y, points[0].z};
        foreach (DoubleVector3 p in points)
        {
            for (int i = 0; i < 3; i++)
            {
                if (min[i] < p[i])
                    min[i] = p[i];
                if (max[i] > p[i])
                    max[i] = p[i];
            }
        }


        DoubleVector3 pos = new DoubleVector3(min[0], min[1], min[2]);
        DoubleVector3 neg = new DoubleVector3(max[0], max[1], max[2]);
        aabb = new AABB(neg, pos);
    }

    public override void SeparateAdjustment(DoubleVector3 normal)
    {
        // CleanAxesRotation(normal);
    }

    public override Matrix3x3 GetInertiaTensor()
    {
        //   return new Matrix3x3();

        Matrix3x3 I = new Matrix3x3();
        double side = halfDims[0] * 2.0;

        I = particle.mass * ((side * side) / 6.0) * I;
        return I;
    }

    public override Matrix3x3 GetInertiaTensorInverted()
    {
        return GetInertiaTensor().GetInverse();
        /*Matrix3x3 I = GetInertiaTensorFraction();
        double side = halfDims[0] * 2.0;

        I = I * particle.inverseMass * (side * side);
        return I;*/
    }

    /* private Matrix3x3 GetInertiaTensorFraction()
     {
         Matrix3x3 I = new Matrix3x3();

         double twoThirds = 2.0 / 3.0;
         double minusOneFourth = -1.0 / 4.0;
         I[0] = twoThirds;
         I[1] = minusOneFourth;
         I[2] = minusOneFourth;
         I[3] = minusOneFourth;
         I[4] = twoThirds;
         I[5] = minusOneFourth;
         I[6] = minusOneFourth;
         I[7] = minusOneFourth;
         I[8] = twoThirds;
         return I;
     }*/

    public DoubleVector3 GetClosestPointOnSurface(DoubleVector3 p)
    {
        DoubleVector3 point = GetClosestPoint(p);


        return particle.position + GetOuterPointFromVec(point - particle.position);
    }

    public DoubleVector3 GetClosestPoint(DoubleVector3 p)
    {
        DoubleVector3 offset = p - particle.position;

        // Debug.DrawLine(particle.position.ToVector3(), p.ToVector3());
        DoubleVector3 point = particle.position;
        // DoubleVector3[] axis = GetAxis(this);
        // ...project d onto that axis to get the distance
        // along the axis of d from the box center
        double dist = DoubleVector3.Dot(offset, particle.right);
        // If distance farther than the box extents, clamp to the box
        if (dist > halfDims.x) dist = halfDims.x;
        if (dist < -halfDims.x) dist = -halfDims.x;
        // Step that distance along the axis to get world coordinate
        point += dist * particle.right;

        dist = DoubleVector3.Dot(offset, particle.up);
        if (dist > halfDims.y) dist = halfDims.y;
        if (dist < -halfDims.y) dist = -halfDims.y;
        point += dist * particle.up;

        dist = DoubleVector3.Dot(offset, particle.forward);
        if (dist > halfDims.z) dist = halfDims.z;
        if (dist < -halfDims.z) dist = -halfDims.z;
        point += dist * particle.forward;

        return point;
    }

    public bool IsPointInside(DoubleVector3 p)
    {
        DoubleVector3 offset = p - particle.position;

        // Debug.DrawLine(particle.position.ToVector3(), p.ToVector3());

        // DoubleVector3[] axis = GetAxis(this);
        // ...project d onto that axis to get the distance
        // along the axis of d from the box center
        double dist = DoubleVector3.Dot(offset, particle.right);
        // If distance farther than the box extents, clamp to the box
        if (dist > halfDims.x) return false;
        if (dist < -halfDims.x) return false;


        dist = DoubleVector3.Dot(offset, particle.up);
        if (dist > halfDims.y) return false;
        if (dist < -halfDims.y) return false;


        dist = DoubleVector3.Dot(offset, particle.forward);
        if (dist > halfDims.z) return false;
        if (dist < -halfDims.z) return false;

        return true;
    }

    //similar to a skybox sample
    public DoubleVector3 GetOuterPointFromVec(DoubleVector3 vec)
    {
        double magnitude = DoubleVector3.Magnitude(vec);

        if (magnitude == 0)
            return particle.position;

        vec = DoubleVector3.Normal(vec);
        DoubleVector3 normalizedDims = DoubleVector3.Normal(new DoubleVector3(halfDims.x, halfDims.y, halfDims.z));
        DoubleVector3 projection = ConvertToAxisCoord(vec);

        double smallestProjectionAbs = Math.Abs(projection.x);// = Math.Min(Math.Min(projectionX, projectionY),projectionZ);
        double smallestProjectionSign = Math.Sign(projection.x);

        if (Math.Abs(projection.y) > smallestProjectionAbs)
        {
            smallestProjectionAbs = Math.Abs(projection.y);
            smallestProjectionSign = Math.Sign(projection.y);
        }
        if (Math.Abs(projection.z) > smallestProjectionAbs)
        {
            smallestProjectionAbs = Math.Abs(projection.z);
            smallestProjectionSign = Math.Sign(projection.z);
        }

        double xScalar = halfDims.x /  smallestProjectionAbs;
        double yScalar = halfDims.y /  smallestProjectionAbs;
        double zScalar = halfDims.z /  smallestProjectionAbs;


        //Debug.Log("mag" + DoubleVector3.Magnitude(scalar * vec));

        return new DoubleVector3(xScalar, yScalar, zScalar) * vec;
    }

    public DoubleVector3 ConvertToAxisCoord(DoubleVector3 vec)
    {
        double projectionX = DoubleVector3.Dot(vec, particle.right);
        double projectionY = DoubleVector3.Dot(vec, particle.up);
        double projectionZ = DoubleVector3.Dot(vec, particle.forward);
        return new DoubleVector3(projectionX, projectionY, projectionZ);
    }

    public override bool CheckCollision(PBDColliderSphere other, ref PBDCollision collision)
    {
        return CheckCollision(other, this, ref collision);
    }

    public override bool CheckCollision(PBDColliderCapsule other, ref PBDCollision collision)
    {
        return CheckCollision(other, this, ref collision);
    }

    public override bool CheckCollision(PBDColliderPlaneY other, ref PBDCollision collision)
    {
        return CheckCollision(other, this, ref collision);
    }

    public override bool CheckCollision(PBDColliderBox other, ref PBDCollision collision)
    {
        return CheckCollision(this, other, ref collision);
    }

    public override PBDColliderType GetColliderType()
    {
        return PBDColliderType.box;
    }

    public static PBDCollision CreateCollision(PBDColliderBox self, PBDColliderBox other, double[] separatingDistances, DoubleVector3[] separatingAxes)
    {
        /*  DoubleVector3 collisionPoint = other.GetClosestPoint(self.particle.position);
          DoubleVector3 otherCollisionPoint = self.GetClosestPoint(other.particle.position);*/


        DoubleVector3 correction, normal;
        DoubleVector3 correctionAxis = separatingAxes[0];
        double minDist = separatingDistances[0];
        int ind = 0;

        for (int i = 1; i <= 5; i++)
        {
            if (separatingDistances[i] < minDist)
            {
                minDist = separatingDistances[i];
                correctionAxis = separatingAxes[i];
                ind = i;
            }
        }


        correction = correctionAxis * minDist;
        normal = DoubleVector3.Normal(correctionAxis);
        //Debug.DrawRay(self.particle.position.ToVector3(), normal.ToVector3() * 10, Color.green, 1f);
        /* self.CleanAxesRotation(normal);
          other.CleanAxesRotation(normal);*/

        bool useFriction = true;
        DoubleVector3 collisionPoint;
        if (ind < 3)
            collisionPoint = self.CalcColisionPoint(other, correction, normal, ref useFriction);
        else if (ind < 6)
            collisionPoint = other.CalcColisionPoint(self, correction, -normal, ref useFriction);
        else
        {
            Debug.LogError("Error in box col");
            collisionPoint = new DoubleVector3(100, 100, 100);
        }


        //   Debug.DrawRay(collisionPoint.ToVector3(), normal.ToVector3(), Color.white, 0.2f);


        self.pos = collisionPoint.ToVector3();

        //other.pos = otherCollisionPoint.ToVector3();
        PBDCollision col = new PBDCollision(self.particle, other.particle, normal, correction, collisionPoint /*, otherCollisionPoint*/);
        col.hasFriction = useFriction;
        return col;
    }

    private  DoubleVector3 CalcColisionPoint(PBDColliderBox other, DoubleVector3 correction, DoubleVector3 normal, ref bool useFriction)
    {
        DoubleVector3 collisionPoint, otherCollisionPoint;
        int axis = 0;
        if (IsParallelWithAxis(other, normal))
        {
            //  Debug.Log("parallel" + normal);
            other.HasParallelAxis(normal, ref axis);
            otherCollisionPoint = other.GetFurthestFaceAlongDirection(-normal,  axis);
            HasParallelAxis(normal, ref axis);
            collisionPoint = GetFurthestFaceAlongDirection(normal, axis);


            if (DoubleVector3.MagnitudeSqr(halfDims) == DoubleVector3.MagnitudeSqr(other.halfDims))
                return DoubleVector3.Lerp(collisionPoint, otherCollisionPoint, 0.5);
            else if (DoubleVector3.MagnitudeSqr(halfDims) > DoubleVector3.MagnitudeSqr(other.halfDims))
                return otherCollisionPoint;
            else
                return collisionPoint;
        }


        DoubleVector3 offset = other.particle.position - particle.position;
        if (other.HasPerpendicularEdge(normal, ref axis))
        {
            collisionPoint = other.GetFurthestEdgeAlongDirection(offset, axis);
            //  Debug.Log("PERP");
            return collisionPoint;
        }
        useFriction = false;
        // Debug.Log("point");
        return other.GetFurthestPointAlongDirection(normal);

        //   Debug.LogError("No colission point was found between " + gameObject.name + " and " + other.gameObject.name);
        //return DoubleVector3.Lerp(particle.position, other.particle.position, 0.5);
    }

    private bool IsParallelWithAxis(PBDColliderBox other, DoubleVector3 axis)
    {
        int axisN = 0;
        return this.HasParallelAxis(axis, ref axisN) && other.HasParallelAxis(axis, ref axisN);
    }

    private bool IsParallel(PBDColliderBox other, double epsilon)
    {
        if (particle.right.IsParallel(other.particle.right, epsilon))
        {
            if (particle.forward.IsParallel(other.particle.forward, epsilon))
                return true;
            if (particle.forward.IsParallel(other.particle.up, epsilon))
                return true;
        }

        if (particle.right.IsParallel(other.particle.forward, epsilon))
        {
            if (particle.forward.IsParallel(other.particle.right, epsilon))
                return true;
            if (particle.forward.IsParallel(other.particle.up, epsilon))
                return true;
        }

        if (particle.right.IsParallel(other.particle.up, epsilon))
        {
            if (particle.forward.IsParallel(other.particle.right, epsilon))
                return true;
            if (particle.forward.IsParallel(other.particle.forward, epsilon))
                return true;
        }

        return false;
    }

    private bool IsParallel(PBDColliderBox other)
    {
        return IsParallel(other, EPSILON);
    }

    private bool HasParallelAxis(DoubleVector3 dir, ref int axis)
    {
        if (particle.right.IsParallel(dir, EPSILON))
        {
            axis = 0;
            return true;
        }
        if (particle.up.IsParallel(dir, EPSILON))
        {
            axis = 1;
            return true;
        }

        if (particle.forward.IsParallel(dir, EPSILON))
        {
            axis = 2;
            return true;
        }

        return false;
    }

    private bool HasPerpendicularEdge(DoubleVector3 dir, ref int axis)
    {
        return HasPerpendicularEdge(dir, ref axis, 0);
    }

    private bool HasPerpendicularEdge(DoubleVector3 dir, ref int axis, double epsilon)
    {
        if (particle.right.IsPerpendicular(dir, epsilon))
        {
            axis = 0;
            return true;
        }
        if (particle.up.IsPerpendicular(dir, epsilon))
        {
            axis = 1;
            return true;
        }
        if (particle.forward.IsPerpendicular(dir, epsilon))
        {
            axis = 2;
            return true;
        }

        return false;
    }

    public static bool TestCrossProductAxises(PBDColliderBox self, PBDColliderBox other, Matrix3x3 R, Matrix3x3 AbsR, double[] separatingDistances, DoubleVector3[] separatingAxes, DoubleVector3 translationInSelfCoords, int startIndex)
    {
        double ra, rb, projectedDistance;
        DoubleVector3 separatingAxis;
        // Test axis L = A0 x B0
        ra = self.halfDims.y  * AbsR[6] + self.halfDims.z * AbsR[3];
        rb = other.halfDims.y * AbsR[2] + other.halfDims.z * AbsR[1];
        projectedDistance = translationInSelfCoords.z * R[3] - translationInSelfCoords.y * R[6];
        separatingAxis = DoubleVector3.Cross(self.particle.right, other.particle.right);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex))
            return false;

        // Test axis L = A0 x B1
        ra = self.halfDims.y  * AbsR[7] + self.halfDims.z  * AbsR[4];
        rb = other.halfDims.x * AbsR[2] + other.halfDims.z * AbsR[0];
        projectedDistance = translationInSelfCoords.z * R[4] - translationInSelfCoords.y * R[7];
        separatingAxis = DoubleVector3.Cross(self.particle.right, other.particle.up);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 1))
            return false;

        // Test axis L = A0 x B2
        ra = self.halfDims.y * AbsR[8] + self.halfDims.z * AbsR[5];
        rb = other.halfDims.x * AbsR[1] + other.halfDims.y * AbsR[0];
        projectedDistance = translationInSelfCoords.z * R[5] - translationInSelfCoords.y * R[8];
        separatingAxis = DoubleVector3.Cross(self.particle.right, other.particle.forward);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 2))
            return false;

        // Test axis L = A1 x B0
        ra = self.halfDims.x  * AbsR[6] + self.halfDims.z  * AbsR[0];
        rb = other.halfDims.y * AbsR[5] + other.halfDims.z * AbsR[4];
        projectedDistance = translationInSelfCoords.x * R[6] - translationInSelfCoords.z * R[0];
        separatingAxis = DoubleVector3.Cross(self.particle.up, other.particle.right);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 3))
            return false;

        // Test axis L = A1 x B1
        ra = self.halfDims.x * AbsR[7] + self.halfDims.z * AbsR[1];
        rb = other.halfDims.x * AbsR[5] + other.halfDims.z * AbsR[3];
        projectedDistance = translationInSelfCoords.x * R[7] - translationInSelfCoords.z * R[1];
        separatingAxis = DoubleVector3.Cross(self.particle.up, other.particle.up);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 4))
            return false;

        // Test axis L = A1 x B2
        ra = self.halfDims.x * AbsR[8] + self.halfDims.z * AbsR[2];
        rb = other.halfDims.x * AbsR[4] + other.halfDims.y * AbsR[3];
        projectedDistance = translationInSelfCoords.x * R[8] - translationInSelfCoords.z * R[2];
        separatingAxis = DoubleVector3.Cross(self.particle.up, other.particle.forward);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 5))
            return false;

        // Test axis L = A2 x B0
        ra = self.halfDims.x * AbsR[3] + self.halfDims.y * AbsR[0];
        rb = other.halfDims.y * AbsR[8] + other.halfDims.z * AbsR[7];
        projectedDistance = translationInSelfCoords.y * R[0] - translationInSelfCoords.x * R[3];
        separatingAxis = DoubleVector3.Cross(self.particle.forward, other.particle.right);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 6))
            return false;
        // Test axis L = A2 x B1
        ra = self.halfDims.x * AbsR[4] + self.halfDims.y * AbsR[1];
        rb = other.halfDims.x * AbsR[8] + other.halfDims.z * AbsR[6];
        projectedDistance = translationInSelfCoords.y * R[1] - translationInSelfCoords.x * R[4];
        separatingAxis = DoubleVector3.Cross(self.particle.forward, other.particle.up);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis,  separatingDistances, separatingAxes, startIndex + 7))
            return false;

        // Test axis L = A2 x B2
        ra = self.halfDims.x * AbsR[5] + self.halfDims.y * AbsR[2];
        rb = other.halfDims.x * AbsR[7] + other.halfDims.y * AbsR[6];
        projectedDistance = translationInSelfCoords.y * R[2] - translationInSelfCoords.x * R[5];
        separatingAxis = DoubleVector3.Cross(self.particle.forward, other.particle.forward);
        if (!TestAxis(ra, rb, projectedDistance, separatingAxis, separatingDistances, separatingAxes, startIndex + 8))
            return false;

        return true;
    }

    public static bool TestFaceParallelAxis(PBDColliderBox a, PBDColliderBox b, DoubleVector3[] axes, Matrix3x3 R, Matrix3x3 AbsR, double[] separatingDistances, DoubleVector3[] separatingAxes, DoubleVector3 translationInSelfCoords, int axisIndex, int index)
    {
        // Test axes L = A0, L = A1, L = A2
        double ra = a.halfDims[axisIndex];
        int matRow = axisIndex * 3;
        double rb = b.halfDims.x * AbsR[matRow] + b.halfDims.y * AbsR[matRow + 1] + b.halfDims.z * AbsR[matRow + 2];
        double projectedDistance = translationInSelfCoords[axisIndex];
        // Debug.DrawRay(b.particle.position.ToVector3(), new DoubleVector3(b.halfDims.x * AbsR[matRow] , b.halfDims.y * AbsR[matRow + 1] , b.halfDims.z * AbsR[matRow + 2]).ToVector3(), Color.blue);
        // Debug.DrawRay(a.particle.position.ToVector3(), (axes[axisIndex] * a.halfDims[axisIndex]).ToVector3(), Color.blue);
        return TestAxis(ra, rb, projectedDistance, axes[axisIndex], separatingDistances, separatingAxes, index);
    }

    public static bool TestFaceParallelAxisB(PBDColliderBox a, PBDColliderBox b, DoubleVector3[] axes, Matrix3x3 R, Matrix3x3 AbsR, double[] separatingDistances, DoubleVector3[] separatingAxes, DoubleVector3 translationInSelfCoords, int axisIndex, int index)
    {
        // Test axes L = A0, L = A1, L = A2
        int matRow = axisIndex * 3;
        double ra = a.halfDims.x * AbsR[matRow] + a.halfDims.y * AbsR[matRow + 1] + a.halfDims.z * AbsR[matRow + 2];
        double rb = b.halfDims[axisIndex];
        double projectedDistance = translationInSelfCoords.x * R[axisIndex] + translationInSelfCoords.y * R[3 + axisIndex] + translationInSelfCoords.z * R[6 + axisIndex];
        // Debug.DrawRay(a.particle.position.ToVector3(), new DoubleVector3(translationInSelfCoords.x * R[axisIndex] , translationInSelfCoords.y * R[3 + axisIndex] , translationInSelfCoords.z * R[6 + axisIndex]).ToVector3(), Color.yellow);
        // Debug.DrawRay(b.particle.position.ToVector3(), (axes[axisIndex] * b.halfDims[axisIndex]).ToVector3(), Color.yellow);
        return TestAxis(ra, rb, projectedDistance, axes[axisIndex], separatingDistances, separatingAxes, index);
    }

    public static bool TestAxis(double radA, double radB, double projectedDistance, DoubleVector3 separatingAxis, double[] separatingDistances, DoubleVector3[] separatingAxes, int index)
    {
        double sum = radA + radB;
        if (Math.Abs(projectedDistance) > sum)
        {
            return false;
        }
        else
        {
            separatingDistances[index] = sum - Math.Abs(projectedDistance); //storing penetration depth
            separatingAxes[index] = separatingAxis * Math.Sign(projectedDistance); //storing penetration depth
        }
        return true;
    }

    public override bool IntersectRay(DoubleRay ray, ref DoubleRayHit hit)
    {
        DoubleVector3 selfCoordsRayPosition = particle.ProjectToSelfCoordinates(ray.point - particle.position);

        DoubleVector3 selfCoordsRayDirection = particle.ProjectToSelfCoordinates(ray.direction);

        // return IntersectRayAABB(ray, ref hit);
        bool result = IntersectRayAABB(new DoubleRay(selfCoordsRayPosition, DoubleVector3.Normal(selfCoordsRayDirection)), ref hit);
        if (!result)
            return false;


        DoubleVector3 worldCoordPoint = particle.position + particle.GetOrientation() * hit.point;
        // Debug.DrawLine(worldCoordPoint.ToVector3(), particle.position.ToVector3(), Color.cyan, 10);

        hit = new DoubleRayHit(this, worldCoordPoint , hit.hitDistance);


        return result;
    }

    private bool IntersectRayAABB(DoubleRay ray, ref DoubleRayHit hit)
    {
        double minDist = 0.0;
        double maxDist = Double.MaxValue;
        double EPSILON = 0.0000001;

        for (int i = 0; i < 3; i++)
        {
            double minBoxBound =  -halfDims[i];
            double maxBoxBound =   halfDims[i];
            if (Math.Abs(ray.direction[i]) < EPSILON)
            {
                if (ray.point[i] < minBoxBound || ray.point[i] > maxBoxBound)
                    return false;
            }
            else
            {
                double ood = 1.0 / ray.direction[i];
                double t1 = (minBoxBound - ray.point[i]) * ood;
                double t2 = (maxBoxBound - ray.point[i]) * ood;

                if (t1 > t2)
                {
                    double aux = t1;
                    t1 = t2;
                    t2 = aux;
                }

                if (t1 > minDist)
                    minDist = t1;
                if (t2 < maxDist)
                    maxDist = t2;
                if (minDist > maxDist)
                    return false;
            }
        }

        DoubleVector3 hitPoint = ray.point + minDist * ray.direction;
        hit = new DoubleRayHit(this, hitPoint, minDist);
        return true;
    }

    public DoubleVector3 GetLowestPoint()
    {
        DoubleVector3 up = new DoubleVector3(Vector3.up);

        //   CleanAxesRotation(up);

        double dot = DoubleVector3.Dot(up, particle.right);
        if (Math.Abs(dot) == 1)
        {
            return particle.position + particle.right * -1 * Math.Sign(dot) * halfDims.x;
        }
        dot = DoubleVector3.Dot(up, particle.up);
        if (Math.Abs(dot) == 1)
        {
            return particle.position + particle.up * -1 * Math.Sign(dot) * halfDims.y;
        }
        dot = DoubleVector3.Dot(up, particle.forward);
        if (Math.Abs(dot) == 1)
        {
            return particle.position + particle.forward * -1 * Math.Sign(dot) * halfDims.z;
        }
        int axis = 0;
        if (HasPerpendicularEdge(up, ref axis))
        {
            return GetFurthestEdgeAlongDirection(up, axis);
        }

        DoubleVector3[] points = new DoubleVector3[8];
        GetVertices(ref points);

        DoubleVector3 lowestPoint = points[0];

        foreach (DoubleVector3 point in points)
        {
            if (point.y < lowestPoint.y)
            {
                lowestPoint = point;
            }
            if (point.y == lowestPoint.y)
            {
            }
        }

        return lowestPoint;
    }

    private DoubleVector3 GetFurthestEdgeAlongDirection(DoubleVector3 dir, int axis)
    {
        // Debug.DrawRay(particle.position.ToVector3(), dir.ToVector3()*10, Color.green, 1f);
        DoubleVector3[] axes = particle.GetAxes();
        DoubleVector3 mainAxis = axes[axis];
        axis++;
        if (axis > 2)
            axis = 0;
        DoubleVector3 secondAxis = axes[axis] * halfDims[axis];
        axis++;
        if (axis > 2)
            axis = 0;
        DoubleVector3 thirdAxis = axes[axis] * halfDims[axis];

        DoubleVector3[] points = new DoubleVector3[4];
        points[0] = secondAxis + thirdAxis;
        points[1] = secondAxis - thirdAxis;
        points[2] = -secondAxis + thirdAxis;
        points[3] = -secondAxis - thirdAxis;
        return GetFurthestPointFromListAlongDirection(points, dir);
    }

    private DoubleVector3 GetFurthestFaceAlongDirection(DoubleVector3 dir,  int axis)
    {
        double dot;
        DoubleVector3[] axes = particle.GetAxes();
        dot = DoubleVector3.Dot(dir, axes[axis]);
        /*  if(Math.Abs(dot) == 1)
          {*/
        return particle.position + axes[axis] * Math.Sign(dot) * halfDims[axis];
        // }

        /*Debug.LogError("Direction " + dot + " Not Perpendicular to any primary axis.",gameObject);
        return dir;*/
    }

    private DoubleVector3 GetFurthestPointAlongDirection(DoubleVector3 dir)
    {
        DoubleVector3[] points = new DoubleVector3[8];
        GetVerticesVectors(ref points);
        return GetFurthestPointFromListAlongDirection(points, dir);
    }

    private DoubleVector3 GetFurthestPointFromListAlongDirection(DoubleVector3[] points, DoubleVector3 dir)
    {
        DoubleVector3 furthestPoint = points[0];
        double furthestDot = DoubleVector3.Dot(DoubleVector3.Normal(points[0]), dir);
        foreach (DoubleVector3 point in points)
        {
            double dot = DoubleVector3.Dot(DoubleVector3.Normal(point), dir);
            if (dot < furthestDot)
            {
                furthestPoint = point;
                furthestDot = dot;
            }
        }
        return particle.position + furthestPoint;
    }

    public void GetVertices(ref DoubleVector3[] points)
    {
        points = new DoubleVector3[8];
        points[0] = particle.position + particle.right * halfDims.x +   particle.up * halfDims.y +      particle.forward * halfDims.z;
        points[1] = particle.position + particle.right * halfDims.x +   particle.up * halfDims.y +      -1 * particle.forward * halfDims.z;
        points[2] = particle.position + particle.right * halfDims.x +   -1 * particle.up * halfDims.y +   particle.forward * halfDims.z;
        points[3] = particle.position + particle.right * halfDims.x +   -1 * particle.up * halfDims.y +   -1 * particle.forward * halfDims.z;
        points[4] = particle.position + -1 * particle.right * halfDims.x + particle.up * halfDims.y +     particle.forward * halfDims.z;
        points[5] = particle.position + -1 * particle.right * halfDims.x + particle.up * halfDims.y +     -1 * particle.forward * halfDims.z;
        points[6] = particle.position + -1 * particle.right * halfDims.x + -1 * particle.up * halfDims.y +  particle.forward * halfDims.z;
        points[7] = particle.position + -1 * particle.right * halfDims.x + -1 * particle.up * halfDims.y +  -1 * particle.forward * halfDims.z;
    }

    public void GetVerticesVectors(ref DoubleVector3[] points)
    {
        points = new DoubleVector3[8];
        points[0] = particle.right * halfDims.x +   particle.up * halfDims.y +      particle.forward * halfDims.z;
        points[1] = particle.right * halfDims.x +   particle.up * halfDims.y +      -1 * particle.forward * halfDims.z;
        points[2] = particle.right * halfDims.x +   -1 * particle.up * halfDims.y +   particle.forward * halfDims.z;
        points[3] = particle.right * halfDims.x +   -1 * particle.up * halfDims.y +   -1 * particle.forward * halfDims.z;
        points[4] = -1 * particle.right * halfDims.x + particle.up * halfDims.y +     particle.forward * halfDims.z;
        points[5] = -1 * particle.right * halfDims.x + particle.up * halfDims.y +     -1 * particle.forward * halfDims.z;
        points[6] = -1 * particle.right * halfDims.x + -1 * particle.up * halfDims.y +  particle.forward * halfDims.z;
        points[7] = -1 * particle.right * halfDims.x + -1 * particle.up * halfDims.y +  -1 * particle.forward * halfDims.z;
    }

    public void CleanPrimaryAxesRotation()
    {
        CleanAxesRotation(new DoubleVector3(Vector3.up));
        CleanAxesRotation(new DoubleVector3(Vector3.right));
        CleanAxesRotation(new DoubleVector3(Vector3.forward));
    }

    public void CleanAxesRotation(DoubleVector3 axis)
    {
        CleanAxisRotation(axis, particle.up);
        CleanAxisRotation(axis, particle.right);
        CleanAxisRotation(axis, particle.forward);
    }

    public void CleanAxisRotation(DoubleVector3 axis, DoubleVector3 direction)
    {
        double dot = DoubleVector3.Dot(axis, direction);
        double absDot = Math.Abs(dot);
        if (absDot > 1.0 - EPSILON && absDot < 1.0)
        {
            if (dot < 0)
            {
                direction *= -1;
                //axis *= -1;
                //   Debug.Log("inverting" + dot);
            }

            DoubleQuaternion q = new DoubleQuaternion(direction, axis);

            // Debug.DrawRay(particle.position.ToVector3(), axis.ToVector3() * 10, Color.red, 0.1f);

            //  Debug.Log("before" + particle.GetOrientation().ToEuler() + " normal: " + axis);
            particle.SetRotation(q * particle.GetOrientation());

            //   Debug.Log("result" + particle.GetOrientation().ToEuler() + " newDot = " + DoubleVector3.Dot(particle.up, axis));
        }
    }
}
