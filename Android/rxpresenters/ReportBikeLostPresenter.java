package kr.co.hksports.hkrider.rxpresenters;

import android.content.Context;
import android.util.Log;

import kr.co.hksports.hkrider.activities.ProtectionLocation;
import kr.co.hksports.hkrider.member.BizLogics;
import kr.co.hksports.hkrider.models.PostBikeLost;
import kr.co.hksports.hkrider.models.ResponseUserInfo;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class ReportBikeLostPresenter { // 추가와 업데이트를 동시에 진행함.
    private Context ctx;
    private HKRiderService mRiderService;
    private String TAG = "ReportBikeLostPresenter";

    public ReportBikeLostPresenter(Context context) {
        ctx = context;
        mRiderService = new HKRiderService();
    }

    public void ReportBikeLost(final PostBikeLost postBikeLost) {

        postBikeLost.header.action = "ReportBikeLost";
        Call<ResponseUserInfo> call = mRiderService.getAPIs().postReportBikeLost(postBikeLost);

        call.enqueue(new Callback<ResponseUserInfo>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseUserInfo> call, Response<ResponseUserInfo> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
//                    Commons.showToast(mRegisterBike, response.body().header.message);
                    Log.e(TAG, "ReportBikeLost: "+ response.body().header.message);
                    Commons.onReportBikeLostError(ctx);
                } else { // 정상 결과의 반영.
                    ResponseUserInfo responseUserInfo = response.body();

                    Log.i(TAG, "responseUserInfo: " + responseUserInfo.body.email);
                    if (responseUserInfo.header.auth_token != null && responseUserInfo.header.auth_token != "") {
                        String token = Commons.getToken(ctx);
                        if (!token.equals(responseUserInfo.header.auth_token))
                            Commons.setToken(ctx, responseUserInfo.header.auth_token);
                    }
                    if (postBikeLost.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }
                    if (responseUserInfo.body.email != null && responseUserInfo.body.email != "") {
                        String email = Commons.getEMail(ctx);
                        if (!email.equals(responseUserInfo.body.email)) {
                            Commons.setEMail(ctx, responseUserInfo.body.email);
                        }
                    }

                    // 전역 변수로 유저 정보를 기억함.
                    BizLogics.setResponseUserInfo(ctx, responseUserInfo);
                    Log.e(TAG, "ReportBikeLost: onSucess");

                    Commons.onReportBikeLostSuccess(ctx);
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseUserInfo> call, Throwable t) {
                Log.e(TAG, "ReportBikeLost: onError: " + t.getMessage());

                Commons.onReportBikeLostFailure(ctx);

            }
        });

    }

}
