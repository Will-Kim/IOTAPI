package kr.co.hksports.hkrider.rxservices;

import kr.co.hksports.hkrider.models.PostBikeLost;
import kr.co.hksports.hkrider.models.PostGetRidingLogs;
import kr.co.hksports.hkrider.models.PostRidingLog;
import kr.co.hksports.hkrider.models.PostRoadBike;
import kr.co.hksports.hkrider.models.PostLogin;
import kr.co.hksports.hkrider.models.PostRegisterMember;
import kr.co.hksports.hkrider.models.ResponseLogin;
import kr.co.hksports.hkrider.models.ResponseRidingLogs;
import kr.co.hksports.hkrider.models.ResponseSimple;
import kr.co.hksports.hkrider.models.ResponseUserInfo;
import okhttp3.MultipartBody;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.Headers;
import retrofit2.http.Multipart;
import retrofit2.http.POST;
import retrofit2.http.Part;

/**
 * API 모음.
 */

public interface HKRiderAPIs {
    @Headers({
            "Cache-Control: max-age=3600",
            "User-Agent: Mozilla/5.0"
    })

    @POST("Login")
    Call<ResponseLogin> postLogin(@Body PostLogin postLogin);

    @POST("RegisterMember")
    Call<ResponseUserInfo> postRegisterMember(@Body PostRegisterMember postRegisterMember);

    @POST("UpdateMember")
    Call<ResponseUserInfo> postUpdateMember(@Body PostRegisterMember postRegisterMember);

    @POST("GetUserInfo")
    Call<ResponseUserInfo> postGetUserInfo(@Body PostLogin postLogin);

    @POST("AddRoadBike")
    Call<ResponseUserInfo> postAddRoadBike(@Body PostRoadBike postRoadBike);

    @POST("RemoveRoadBike")
    Call<ResponseUserInfo> postRemoveRoadBike(@Body PostRoadBike postRoadBike);

    @Multipart
    @POST("UploadBikeImage")
    Call<ResponseSimple> postUploadBikeImage(
            @Part MultipartBody.Part file
    );

    @POST("AddRidingLog")
    Call<ResponseUserInfo> postAddRidingLog(@Body PostRidingLog postRidingLog);

    @POST("GetRidingLogs")
    Call<ResponseRidingLogs> postGetRidingLog(@Body PostGetRidingLogs postGetRidingLogs);

    @POST("AddBikeLost")
    Call<ResponseUserInfo> postAddBikeLost(@Body PostBikeLost postBikeLost);

    @POST("RemoveBikeLost")
    Call<ResponseUserInfo> postRemoveBikeLost(@Body PostBikeLost postBikeLost);

    @POST("ReportBikeLost")
    Call<ResponseUserInfo> postReportBikeLost(@Body PostBikeLost postBikeLost);

    @POST("GetBikeLost")
    Call<PostBikeLost> postGetBikeLost(@Body PostBikeLost postBikeLost);

}
