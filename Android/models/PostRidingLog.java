package kr.co.hksports.hkrider.models;

/**
 * Created by WillKim on 24/04/2017.
 */

public class PostRidingLog {
    public HKRheader header ;
    public RidingLog body ;

    // 메일주소와 패스워드만 먼저 세팅할 때.
    public PostRidingLog() {
        if (header == null) header = new HKRheader();
        if (body == null) body = new RidingLog();
    }
}
