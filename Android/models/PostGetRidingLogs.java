package kr.co.hksports.hkrider.models;

/**
 * Created by WillKim on 24/04/2017.
 */

public class PostGetRidingLogs {
    public HKRheader header ;
    public RequestLogs body ;

    public PostGetRidingLogs() {
        if (header == null) header = new HKRheader();
        if (body == null) body = new RequestLogs();
    }

    public class RequestLogs {
        public long from_date;
        public long to_date;
        public String mac_address;
        public long remove_id;

        public void SetDates(long FromDate, long ToDate) {
            from_date = FromDate;
            to_date = ToDate;
        }

        public void SetMacAddress(String macAddress) {
            mac_address = macAddress;
        }

        public void SetRemoveId(long id) { remove_id = id; }
    }

}
