package kr.co.hksports.hkrider.models;

import android.content.Context;
import android.util.Log;

import kr.co.hksports.hkrider.utils.Commons;

/**
 * Created by WillKim on 06/04/2017.
 */

public class BikeLost {
    public int id ;
    public String username;
    public String nickname ;
    public String phone_no;
    public String extra_info;
    public String modelname ;
    public String color_size ;
    public String frame_no ;
    public String characteristics ;

    public String location_lost;
    public String status;           // L:losted, F:found, C:canceled
    public long dt_lost;

    public String device_id ;       // 블루투스 맥어드레스

}
