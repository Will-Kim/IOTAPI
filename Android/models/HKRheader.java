package kr.co.hksports.hkrider.models;

import android.content.Context;

import kr.co.hksports.hkrider.utils.Commons;

/**
 * Created by WillKim on 06/04/2017.
 */

public class HKRheader {
    public String action;
    public String auth_token ;
    public int result_code ; //OK:0,Update required:1, Error:100~
    public String message ; // messages for Error etc
    public String app_key       ;
    public String auth_device_id ;
    public String client_version ;
    public String client_os     ;
    public String client_market;
    public int mailbox_idx ; //메일박스가 어디까지 쌓여 있는지 번호
    public String ret_string; // 상황에 따라서 추가된 로그의 ID를 돌려 받거나 할 수 있다. action에 따라서 정의가 된다.

    // 헤더 기본 값 세팅.
    public void setHeaderDefault(Context context) {
        app_key = "";
        auth_device_id = Commons.getFCMToken(context); // 나중에 Push Notification Service id로 수정함.
        client_market = "self"; // 자체 로그인
        client_os = "Android " + Commons.currentVersion();
        client_version = String.valueOf(Commons.getAppVersion(context)); // 숫자임
        auth_token = Commons.getToken(context);
    }

}
