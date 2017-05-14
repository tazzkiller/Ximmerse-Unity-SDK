//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

namespace Ximmerse{
	public partial class MathUtil {

		public const float kPi=Mathf.PI,kPiOver2=Mathf.PI/2f;

		public static float Normalize(float min, float max, float value) {
			value=Mathf.Clamp(value,min,max);
			return (value-min)/(max-min);
		}

		/// <summary>
		/// The angle of Vector2(x,y) in degrees.
		/// </summary>
		public static float GetAngle(float x,float y){
			if(x==0f){
				if(y>0f){
					return Mathf.Rad2Deg*kPiOver2;
				}else if(y<0f){
					return Mathf.Rad2Deg*-kPiOver2;
				}
			}else {
				float a=Mathf.Atan(y/x);
				if(a>0f){
					if(x<0f){
						a+=kPi;
					}
				}else if(a<0){
					if(x<0f){
						a+=kPi;
					}
				}
				return Mathf.Rad2Deg*a;
			}
			return Mathf.Rad2Deg*0f;
		}

		public static int Repeat(int min,int max,int value) {
			return min+((value-min)- (Mathf.FloorToInt((value-min)/ (max-min)) * (max-min)));
		}

		public static float Repeat(float min,float max,float value) {
			return min+((value-min)- (Mathf.Floor((value-min)/ (max-min)) * (max-min)));
		}

		public static Rect MergeRects(params Rect[] rects) {
			int i=0,imax=rects.Length;
			Vector2 min=new Vector2(rects[i].xMin,rects[i].yMin),
				    max=new Vector2(rects[i].xMax,rects[i].yMax);
			++i;
			for(;i<imax;++i) {
				min.Set(
					Mathf.Min(min.x,rects[i].xMin),
					Mathf.Min(min.y,rects[i].yMin)
				);
				max.Set(
					Mathf.Max(max.x,rects[i].xMax),
					Mathf.Max(max.y,rects[i].yMax)
				);
			}
			return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
		}

		public static bool HorizontalSplitRect(Rect[] dest,int destOffset,ref Rect src,float lhs=-1f,float rhs=-1f,float padding=4f) {
			Vector2 position=src.position;
			Vector2 size=src.size;
			if(lhs==-1f&&rhs==-1f) {
				//
				size.x=(size.x-padding)/2f;
				dest[destOffset+0].position=position;
				dest[destOffset+0].size=size;
				//
				position.x+=padding+size.x;
				dest[destOffset+1].position=position;
				dest[destOffset+1].size=size;
			}else if(rhs==-1f) {
				//
				dest[destOffset+0].position=position;
				dest[destOffset+0].width=lhs;dest[destOffset+0].height=size.y;
				//
				size.x=(size.x-lhs-padding);
				position.x+=lhs+padding;
				dest[destOffset+1].position=position;
				dest[destOffset+1].size=size;
			}else if(lhs==-1f) {
				//
				size.x=(size.x-padding-rhs);
				dest[destOffset+0].position=position;
				dest[destOffset+0].size=size;
				//
				position.x+=size.x+padding;
				dest[destOffset+1].position=position;
				dest[destOffset+1].width=rhs;dest[destOffset+1].height=size.y;
			}else {
				return false;
			}
			return true;
		}


		#region Circle/Sphere

		// Taken from http://blog.csdn.net/lijiayu2015/article/details/52541730

		struct CircleData {
			public Vector2 center;
			public float radius;
		};

		static CircleData findCircle2(Vector2 pt1,Vector2 pt2,Vector2 pt3) {
			//
			float A1, A2, B1, B2, C1, C2, temp;
			A1=pt1.x-pt2.x;
			B1=pt1.y-pt2.y;
			C1=(Mathf.Pow(pt1.x,2)-Mathf.Pow(pt2.x,2)+Mathf.Pow(pt1.y,2)-Mathf.Pow(pt2.y,2))/2;
			A2=pt3.x-pt2.x;
			B2=pt3.y-pt2.y;
			C2=(Mathf.Pow(pt3.x,2)-Mathf.Pow(pt2.x,2)+Mathf.Pow(pt3.y,2)-Mathf.Pow(pt2.y,2))/2;
			//
			temp=A1*B2-A2*B1;
			//
			CircleData CD;
			//
			if(temp==0) {
				//
				CD.center.x=pt1.x;
				CD.center.y=pt1.y;
			} else {
				//
				CD.center.x=(C1*B2-C2*B1)/temp;
				CD.center.y=(A1*C2-A2*C1)/temp;
			}

			CD.radius=Mathf.Sqrt((CD.center.x-pt1.x)*(CD.center.x-pt1.x)+(CD.center.y-pt1.y)*(CD.center.y-pt1.y));
			return CD;
		}


		// Taken from http://blog.csdn.net/yrc1993/article/details/7907894

		static void get_xyz(float x1,float y1,float z1,
			 float x2,float y2,float z2,
			 float x3,float y3,float z3,
			 float x4,float y4,float z4,
			 out float x,out float y,out float z
		){
			float a11, a12, a13, a21, a22, a23, a31, a32, a33, b1, b2, b3, d, d1, d2, d3;
			a11=2*(x2-x1);
			a12=2*(y2-y1);
			a13=2*(z2-z1);
			a21=2*(x3-x2);
			a22=2*(y3-y2);
			a23=2*(z3-z2);
			a31=2*(x4-x3);
			a32=2*(y4-y3);
			a33=2*(z4-z3);
			b1=x2*x2-x1*x1+y2*y2-y1*y1+z2*z2-z1*z1;
			b2=x3*x3-x2*x2+y3*y3-y2*y2+z3*z3-z2*z2;
			b3=x4*x4-x3*x3+y4*y4-y3*y3+z4*z4-z3*z3;
			d=a11*a22*a33+a12*a23*a31+a13*a21*a32-a11*a23*a32-a12*a21*a33-a13*a22*a31;
			d1=b1*a22*a33+a12*a23*b3+a13*b2*a32-b1*a23*a32-a12*b2*a33-a13*a22*b3;
			d2=a11*b2*a33+b1*a23*a31+a13*a21*b3-a11*a23*b3-b1*a21*a33-a13*b2*a31;
			d3=a11*a22*b3+a12*b2*a31+b1*a21*a32-a11*b2*a32-a12*a21*b3-b1*a22*a31;
			x=d1/d;
			y=d2/d;
			z=d3/d;
		}

		public static bool CalculateCircle(
			Vector2 p0,
			Vector2 p1,
			Vector2 p2,
			out Vector2 center,
			out float radius
		) {
			center=Vector2.zero;
			radius=0f;
			try {
				CircleData cd=findCircle2(p0,p1,p2);
				if(cd.center==p0) { return false;}
				center=cd.center;
				radius=cd.radius;
			}catch {
				return false;
			}
			return true;
		}

		public static bool CalculateSphere(
			Vector3 p0,
			Vector3 p1,
			Vector3 p2,
			Vector3 p3,
			out Vector3 center,
			out float radius
		) {
			center=Vector3.zero;
			radius=0f;
			try {
				get_xyz(
					p0.x,p0.y,p0.z,
					p1.x,p1.y,p1.z,
					p2.x,p2.y,p2.z,
					p3.x,p3.y,p3.z,
					out center.x,out center.y,out center.z
				);
				radius=(p0-center).magnitude;
			}catch {
				return false;
			}
			return true;
		}

		#endregion Circle/Sphere

	}
}
