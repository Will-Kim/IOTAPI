package kr.co.hksports.hkrider.models;

import android.content.Context;
import android.util.Log;

import kr.co.hksports.hkrider.utils.Commons;

/**
 * Created by WillKim on 06/04/2017.
 */

public class RoadBike {
    public int id ;
    public int user_id ; // 유저의 Key
    public String nickname ;
    public String device_id ; // 블루투스 맥어드레스
    public int type ;
    public String typename ;
    public String wheel_size ;
    public String modelname ;
    public String color_size ;
    public String frame_no ;
    public String characteristics ;
    public String pictures ;
    public int avatar_level ;
    public int riding_score ;
    public int riding_count ;
    public int distance_accumulated ;
    public int time_accumulated ;
    public int average_distance ;
    public int average_riding_time ;
    public int calories_accumulated ;
    public int average_calories ;
    public String lost;
    public String last_gps;

    public void init(Context ctx) {
        String recentMac = Commons.getRecentMac(ctx);
        if(recentMac.length()==0) Commons.setRecentMac(ctx, device_id);
        if (recentMac == null || recentMac.length()==0 || device_id == null || !recentMac.equals(device_id)) {
            return;
        }
        // 같은 경우
        Commons.setData(ctx, "score_next_level", String.valueOf(Commons.LevelConditions[avatar_level]-riding_score));
        Commons.setData(ctx, "level_progress", String.valueOf(riding_score-Commons.LevelConditions[avatar_level-1]));
        Commons.setData(ctx, "riding_score", String.valueOf(riding_score));
        Commons.setData(ctx, "level", String.valueOf(avatar_level));
        Commons.setData(ctx, "distance_accum", String.valueOf(distance_accumulated/1000) + " km");
        Commons.setData(ctx, "time_accum", String.format("%02d:%02d:%02d", (int)(time_accumulated/3600),
                (int)(time_accumulated/60 % 60), (int)(time_accumulated % 60)));
        Commons.setData(ctx, "average_dist", String.valueOf(average_distance/1000) + " km");
        Commons.setData(ctx, "average_time", String.format("%02d:%02d:%02d", (int)(average_riding_time/3600),
                (int)(average_riding_time/60 % 60), (int)(average_riding_time % 60)));
        Commons.setData(ctx, "total_cnt", String.valueOf(riding_count)+" 번");
        Commons.setData(ctx, "calories_accum", String.valueOf(calories_accumulated));

        Commons.setData(ctx, "BikeNickname", nickname);
        Commons.setData(ctx, "BikeTypeIdx", String.valueOf(type));
        Commons.setData(ctx, "WheelSize", wheel_size);
        Commons.setData(ctx, "WheelSizeIdx", Commons.getWheelSizeIndex(wheel_size)==-1?"":String.valueOf(Commons.getWheelSizeIndex(wheel_size)));
        Commons.setData(ctx, "Modelname", modelname);
        Commons.setData(ctx, "ColorSize", color_size);
        Commons.setData(ctx, "FrameNo", frame_no);
        Commons.setData(ctx, "Characteristics", characteristics);
        Commons.setData(ctx, "ImageFileName", pictures);

        lost = lost.replace(" ", "");
        Commons.setData(ctx, "Lost", lost); //분실여부 N, Y, 공백제거.
        //  마지막 위치의 시간과 주소 정보를 가지고 옴. 밀리세컨드시간 + ":" + Lat +","+Long.
        if(last_gps == null) last_gps = "";
        last_gps = last_gps.replace(" ", "");
        Log.e("RoadBike.init()", "last_gps: " + last_gps);
        String[] strArray = last_gps.split(":");
        if (strArray.length==2) { // 시간과 위치 정보가 다 있는 경우.
            Log.e("ROADBIKE_init", strArray[0] +","+strArray[1]);
            Commons.setLastGPSmillis(ctx, strArray[0]);
            Commons.setLastGPSpoint(ctx, strArray[1]);
        } else { // 시간 정보가 없는 경우. --> 예전 버전에서는 위치만 올라갔음.
            Commons.setLastGPSmillis(ctx, ""); // 이전에는 시간을 보관하지 않았음.
            Commons.setLastGPSpoint(ctx, last_gps);
        }

    }
}
