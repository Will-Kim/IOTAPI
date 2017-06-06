package kr.co.hksports.hkrider.models;

/**
 * Created by WillKim on 13/04/2017.
 */

public class PostBikeLost {
    public HKRheader header ;
    public BikeLost body ;

    public PostBikeLost() {
        if (header == null) header = new HKRheader();
        if (body == null) body = new BikeLost();
    }

}
