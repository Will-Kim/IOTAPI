package kr.co.hksports.hkrider.models;

/**
 * Created by WillKim on 13/04/2017.
 */

public class PostRoadBike {
    public HKRheader header ;
    public RoadBike body ;

    // 메일주소와 패스워드만 먼저 세팅할 때.
    public PostRoadBike() {
        if (header == null) header = new HKRheader();
        if (body == null) body = new RoadBike();
    }

}
