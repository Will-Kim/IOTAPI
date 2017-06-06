package kr.co.hksports.hkrider.models;

import android.content.Context;
import android.content.Intent;
import android.util.Log;

import com.google.android.gms.maps.model.LatLng;

import java.io.Serializable;
import java.text.DecimalFormat;

import kr.co.hksports.hkrider.utils.Commons;
import kr.co.hksports.hkrider.utils.log;

/**
 * Created by WillKim on 19/04/2017.
 */

public class RidingLog implements Serializable {
    public long id;
    public int riding_score;
    public int distance; // meter
    public long duration; //
    public long dt_start;
    public long dt_end;
    public int average_speed; // m/sec
    public String max_speed;
    public int calories_burn;
    private String gps_records;
    public String start_location;
    public String end_location;
    private String mac_address;
    private String debug_data;
    private String last_gps;

    public void init(Context ctx, long dt) {
        riding_score = 0;
        distance = 0;
        duration = 0;
        dt_start = dt;
        dt_end = 0;
        average_speed = 0;
        max_speed = "0";
        calories_burn = 0;
        start_location = "";
        end_location = "";
        gps_records = "";
        mac_address = "";
        debug_data = "";
        last_gps = "";

        Commons.setCurrentSpeed(ctx, new DecimalFormat("0.0").format(0)); // km/hour
        Commons.setData(ctx, "DistanceAcc", new DecimalFormat("0.0").format(0));
        Commons.setData(ctx, "TimeAcc", String.format("%02d:%02d:%02d", 0, 0, 0));
        Commons.setData(ctx, "CaloriesBurned", String.valueOf(0));
        Commons.setData(ctx, "AverageSpeed", new DecimalFormat("0.0").format(0));

    }

    public long getId() {
        return id;
    }

    public String getMacAddress() {
        return mac_address;
    }

    public String getGPSreocrds() {
        return gps_records;
    }

    public void setMacAddress(String macAddress) {
        mac_address = macAddress;
    }

    public void addRidingData(Context ctx, String stat, String macAddress, int batLevel, int sequence, int rssi, long latency,
                              int gap, int wheelSize, int accelChange, long currentMillis, long gapMillis, long pausingTime,
                              String resultStr, boolean letsGetStarted) {

        resultStr += "," + String.valueOf(wheelSize);
        double currentSpeed_ms = (latency == 0 ? 0 : ((double) wheelSize * 1.0 / 1000.0) / ((double) latency / 1000000)); // meter per second.
        double currentSpeed = currentSpeed_ms * (60.0 * 60 / 1000.0); // Km/hour

        resultStr += "," + String.valueOf(gap);
        resultStr += "," + String.valueOf(gapMillis);
        resultStr += "," + String.valueOf(currentSpeed);
        resultStr += "," + String.valueOf(accelChange);

        // 저장.
        Commons.setBattery(ctx, batLevel);
        Commons.setCurrentSpeed(ctx, new DecimalFormat("0.#").format(currentSpeed));
        Commons.setAccelChanges(ctx, accelChange);

        distance += ((gap * wheelSize)/1000.0);
        duration = (currentMillis - dt_start - pausingTime)/1000; // 초 단위로 저장한다.

        resultStr += "," + String.valueOf(distance);
        resultStr += "," + String.valueOf(duration);

        double BMR = 0, w = 0, h = 0, a = 0;
        if (Commons.getData(ctx, "sex").equals("M")) {
            w = Commons.getData(ctx, "weight").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "weight"));
            h = Commons.getData(ctx, "height").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "height"));
            a = Commons.getData(ctx, "age").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "age"));
            BMR = 13.7 * w + 5.0 * h - 6.7 * a + 66;
        } else {
            w = Commons.getData(ctx, "weight").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "weight"));
            h = Commons.getData(ctx, "height").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "height"));
            a = Commons.getData(ctx, "age").length() == 0 ? 0 : Double.valueOf(Commons.getData(ctx, "age"));
            BMR = 9.6 * w + 1.85 * h - 4.7 * a + 655;
        }
        resultStr += "," + String.valueOf(BMR);

        // 주행 거리로 운동강도가 결정되는지 속도인지 확인 필요.
        double ExerciseStress = (distance / 1000 < 15 ? 8.0 : distance / 1000 < 30.0 ? 10.0 : 12.0);
        // 칼로리는 KCal가 아니라 그냥 Cal일 듯. 확인 필요.
        calories_burn = (int) ((double) BMR * ExerciseStress * ((double) duration / 60)) / 1440;
        resultStr += "," + String.valueOf(ExerciseStress);
        resultStr += "," + String.valueOf(calories_burn);

        if (currentSpeed > Float.valueOf(max_speed)) max_speed = new DecimalFormat("0.0").format(currentSpeed); // ms
        Commons.setCurrentSpeed(ctx, new DecimalFormat("0.0").format(currentSpeed)); // km/hour
        Commons.setData(ctx, "DistanceAcc", new DecimalFormat("0.0").format(distance / 1000.0));
        Commons.setData(ctx, "TimeAcc", String.format("%02d:%02d:%02d", (int) (duration / (3600)),
                (int) ((duration /60) % 60), (int) (duration % 60)));
        Commons.setData(ctx, "CaloriesBurned", String.valueOf(calories_burn));

        average_speed = (int) (duration) == 0 ? 0 : (int) (((double) distance / 1.0) / ((double) duration / 1)); // m/sec
        Commons.setData(ctx, "AverageSpeed", new DecimalFormat("0.0").format(((double) distance / 1000.0) / ((double) duration / (3600))));
        Log.e("RidingLog", "AVG: "+average_speed+"="+ ((double) distance / 1.0)+"/"+((double) duration));

        resultStr += "," + Commons.getData(ctx, "AverageSpeed");
        dt_end = currentMillis;

        Log.e("RidingLog", "LOG:" + distance + "," + duration + "," + pausingTime / 1000 + "," + calories_burn);


        Intent ib = new Intent(letsGetStarted ?
                Commons.HKRIDER.NEW_RIDING_START : Commons.HKRIDER.NEW_RIDING_RECORD);
        ib.putExtra("timeSeconds", duration);
        ctx.sendBroadcast(ib);

    }

    // 라이딩점수=총누적거리 1km당 1점+총누적시간 1분당 1점+라이딩횟수 당 5점+기록최고속도
    public boolean saveLog(Context ctx, long dur, String debugData) {

        boolean ret = saveLogTemp(ctx, dur, debugData);
        if (!ret) return false;

        // 인터넷 체크
        start_location = "";
        end_location = "";
        String[] gpsPoints = gps_records.split(",");
        int idx = gpsPoints.length-1;
        if (gpsPoints.length >= 4) {
            LatLng sPoint = new LatLng(Double.valueOf(gpsPoints[0]), Double.valueOf(gpsPoints[1]));
            Commons.setGeoCoder(ctx, "Start", sPoint);
            start_location = Commons.getData(ctx, "StartAddress1") + "," +
                    Commons.getData(ctx, "StartAddress2") + "," +
                    Commons.getData(ctx, "StartAddress3") + "," +
                    Commons.getData(ctx, "StartAddress4");

            LatLng ePoint = new LatLng(Double.valueOf(gpsPoints[idx-1]), Double.valueOf(gpsPoints[idx]));
            Commons.setGeoCoder(ctx, "End", ePoint);
            end_location = Commons.getData(ctx, "EndAddress1") + "," +
                    Commons.getData(ctx, "EndAddress2") + "," +
                    Commons.getData(ctx, "EndAddress3") + "," +
                    Commons.getData(ctx, "EndAddress4");
        }

        Log.e("RidingLog", "SaveLog: " + duration + "," + distance);

        return true;

    }

    // 라이딩점수=총누적거리 1km당 1점+총누적시간 1분당 1점+라이딩횟수 당 5점+기록최고속도
    public boolean saveLogTemp(Context ctx, long dur, String debugData) {
        // 인터넷 체크를 해서 팝업창..
        Log.e("RidingLog", "saveLog() called");
        riding_score = distance / 1000 + (int) duration / 60 + 5; // 최고기록을 냈을때 증가분?
        if (distance == 0 || duration == 0 || riding_score == 0) return false;

        //  마지막 위치의 주소 정보를 가지고 옴.
        String gpsLast = Commons.getLastGPSpoint(ctx);
        last_gps = String.valueOf(dt_end) + ":" + gpsLast;
        last_gps.replace(" ", "");

        gps_records = Commons.getData(ctx, "RoutePoints");

        debug_data = debugData;

        return true;
    }

    public void printRidingLog() {
        Log.e("RidingLog", "id: "+id);
        Log.e("RidingLog", "riding_score: "+riding_score);
        Log.e("RidingLog", "distance: "+distance); // meter
        Log.e("RidingLog", "duration: "+duration) ; //
        Log.e("RidingLog", "dt_start: "+dt_start);
        Log.e("RidingLog", "dt_end: "+dt_end);
        Log.e("RidingLog", "average_speed: "+average_speed); // m/sec
        Log.e("RidingLog", "max_speed: "+max_speed);
        Log.e("RidingLog", "calories_burn: "+calories_burn);
        Log.e("RidingLog", "gps_records: "+gps_records);
        Log.e("RidingLog", "start_location: "+start_location);
        Log.e("RidingLog", "end_location: "+end_location);
        Log.e("RidingLog", "mac_address: "+mac_address);
        Log.e("RidingLog", "debug_data: "+debug_data);
        Log.e("RidingLog", "last_gps: "+last_gps);

    }
}
