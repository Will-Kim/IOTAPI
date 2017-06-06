package kr.co.hksports.hkrider.models;

import android.content.Context;
import android.util.Log;

import com.google.android.gms.maps.model.LatLng;

import java.util.ArrayList;
import java.util.List;

import kr.co.hksports.hkrider.utils.Commons;

/**
 * Created by WillKim on 25/04/2017.
 */

public class RoutePoints {
    private Context ctx;
    private List<LatLng> gpsPoints;
    private String strRoutePoints;

    public RoutePoints(Context context) {
        ctx = context;
        if (gpsPoints == null) {
            gpsPoints = new ArrayList();
            strRoutePoints = "";
        } else {
            gpsPoints.clear();
            strRoutePoints = "";
        }
    }

    public List<LatLng> getAllPoints() {
        return gpsPoints;
    }

    //데이터를 생성하는 곳은 꼭 한 곳이어야 함. --> 반드시 RiderGPS에서만 추가되어야 함. (다른 곳에서 건드리면 안됨)
    public String add(LatLng gpsPoint) {
        gpsPoints.add(gpsPoint);
        String strPoint = String.valueOf(gpsPoint.latitude) + "," + String.valueOf(gpsPoint.longitude);
        strRoutePoints += (strRoutePoints.length()==0 ? "" : ",") + strPoint;
        Commons.setData(ctx, "RoutePoints", strRoutePoints);

        if (gpsPoints.size() <=1) {
            Commons.setData(ctx, "FirstGPSpoint", strPoint);
        }
        Commons.setLastGPSpoint(ctx, strPoint);

        Log.e("RoutePoints", "GPS Points " + gpsPoints.size() + ": " + strRoutePoints);

        return strPoint;
    }

    // 문자열로 부터 하나의 GPS 위치를 추가하고 index를 돌려준다. index가 -1이면 오류.
    public int addFromString(String stringPoint) {
        String[] strPoint = stringPoint.split(",");
        if(strPoint.length!=2) return -1;
        LatLng aPoint = new LatLng(Double.valueOf(strPoint[0]), Double.valueOf(strPoint[1]));
        gpsPoints.add(aPoint);
        return gpsPoints.size()-1;
    }


    // gps 위치 가지고 오기.
    public LatLng get(int index) {
        if(gpsPoints.size() >= index) {
            return gpsPoints.get(index);
        }
        return null;
    }

    public LatLng getLast() {
        int i = gpsPoints.size();
        if(i > 0) {
            return gpsPoints.get(i-1);
        } else {
            String gpsLast = Commons.getLastGPSpoint(ctx);
            String[] strPoint = gpsLast.split(",");
            if(strPoint.length==2) {
                LatLng aPoint = new LatLng(Double.valueOf(strPoint[0]), Double.valueOf(strPoint[1]));
                return aPoint;
            }
        }
        return null;
    }

    public LatLng getFirst() {
        int i = gpsPoints.size();
        if(i > 0) {
            return gpsPoints.get(0);
        }
        return null;
    }

    public String toString() {
        return strRoutePoints;
    }

    public void fromString(String fromString) {
        gpsPoints.clear();
        strRoutePoints = fromString;
        String[] strParsed = fromString.split(",");
        for (int i=0; i<strParsed.length;i+=2) {
            LatLng aPoint = new LatLng(Double.valueOf(strParsed[i]), Double.valueOf(strParsed[i+1]));
            gpsPoints.add(aPoint);
        }
    }

    public int size() {
        return gpsPoints.size();
    }

    public void clear() {
        gpsPoints.clear();
    }
}
