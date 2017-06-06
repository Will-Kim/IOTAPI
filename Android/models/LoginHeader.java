package kr.co.hksports.hkrider.models;

import android.content.Context;

import kr.co.hksports.hkrider.utils.Commons;

/**
 * Created by WillKim on 06/04/2017.
 */
public class LoginHeader
{
    public String action        ;
    public String app_key       ;
    public String auth_token    ;
    public String auth_device_id ;
    public String client_version ;
    public String client_os     ;
    public String client_market ;

    public void init(Context context) {
        action = "Login";
        app_key = "";
        auth_device_id = Commons.getFCMToken(context); // 나중에 Push Notification Service id로 수정함.
        client_market = "self"; // 자체 로그인
        client_os = "Android " + Commons.currentVersion();
        client_version = String.valueOf(Commons.getAppVersion(context)); // 숫자임
        auth_token = Commons.getToken(context);

    }
}
