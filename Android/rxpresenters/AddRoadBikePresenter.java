package kr.co.hksports.hkrider.rxpresenters;

import android.util.Log;

import kr.co.hksports.hkrider.member.BizLogics;
import kr.co.hksports.hkrider.member.RegisterBike;
import kr.co.hksports.hkrider.models.PostRoadBike;
import kr.co.hksports.hkrider.models.ResponseUserInfo;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class AddRoadBikePresenter { // 추가와 업데이트를 동시에 진행함.
    private RegisterBike mRegisterBike;
    private HKRiderService mRiderService;
    private String TAG = "AddRoadBikePresenter";

    public AddRoadBikePresenter(RegisterBike RegisterBikeActivity) {
        mRegisterBike = RegisterBikeActivity;
        mRiderService = new HKRiderService();
    }

    public void RegisterBike(final PostRoadBike postRoadBike) {

        postRoadBike.header.action = "AddRoadBike";
        Call<ResponseUserInfo> call = mRiderService.getAPIs().postAddRoadBike(postRoadBike);

        call.enqueue(new Callback<ResponseUserInfo>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseUserInfo> call, Response<ResponseUserInfo> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
//                    Commons.showToast(mRegisterBike, response.body().header.message);
                    mRegisterBike.onAddRoadBikeError(response.body().header.message);
                } else { // 정상 결과의 반영.
                    ResponseUserInfo responseUserInfo = response.body();

                    Log.i(TAG, "responseUserInfo: " + responseUserInfo.body.email);
                    if (responseUserInfo.header.auth_token != null && responseUserInfo.header.auth_token != "") {
                        String token = Commons.getToken(mRegisterBike);
                        if (!token.equals(responseUserInfo.header.auth_token))
                            Commons.setToken(mRegisterBike, responseUserInfo.header.auth_token);
                    }
                    if (postRoadBike.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }
                    if (responseUserInfo.body.email != null && responseUserInfo.body.email != "") {
                        String email = Commons.getEMail(mRegisterBike);
                        if (!email.equals(responseUserInfo.body.email)) {
                            Commons.setEMail(mRegisterBike, responseUserInfo.body.email);
                        }
                    }

                    // 전역 변수로 유저 정보를 기억함.
                    BizLogics.setResponseUserInfo(mRegisterBike, responseUserInfo);

                    mRegisterBike.onAddRoadBikeSuccess();
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseUserInfo> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());
                mRegisterBike.onAddRoadBikeFail();

            }
        });

    }

}
