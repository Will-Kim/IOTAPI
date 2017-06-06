package kr.co.hksports.hkrider.rxpresenters;

import android.content.Context;
import android.util.Log;

import kr.co.hksports.hkrider.RiderLogin;
import kr.co.hksports.hkrider.member.BizLogics;
import kr.co.hksports.hkrider.member.RegisterMember;
import kr.co.hksports.hkrider.models.PostRegisterMember;
import kr.co.hksports.hkrider.models.ResponseUserInfo;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class UpdateMemberPresenter {
    private Context ctx;
    private RegisterMember mRegisterMember;
    private HKRiderService mRiderService;
    private String TAG = "LoginPresenter";

    public UpdateMemberPresenter(Context context) {
        ctx = context;
        mRiderService = new HKRiderService();
    }

    public void updateMember(final PostRegisterMember postRegisterMember, RegisterMember registerMemmber) {
        mRegisterMember = registerMemmber;

        postRegisterMember.header.action = "UpdateMember";
        Call<ResponseUserInfo> call = mRiderService.getAPIs().postUpdateMember(postRegisterMember);

        call.enqueue(new Callback<ResponseUserInfo>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseUserInfo> call, Response<ResponseUserInfo> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
                    mRegisterMember.onUpdateMemberErr(response.body().header.message);
//                    Commons.showToast(mRegisterMember, response.body().header.message);
                } else { // 정상 결과의 반영.
                    ResponseUserInfo responseUserInfo = response.body();

                    Log.i(TAG, "responseUserInfo: " + responseUserInfo.body.email);
                    if (responseUserInfo.header.auth_token != null && responseUserInfo.header.auth_token != "") {
                        String token = Commons.getToken(mRegisterMember);
                        if (!token.equals(responseUserInfo.header.auth_token))
                            Commons.setToken(mRegisterMember, responseUserInfo.header.auth_token);
                    }
                    if (postRegisterMember.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }
                    if (responseUserInfo.body.email != null && responseUserInfo.body.email != "") {
                        String email = Commons.getEMail(mRegisterMember);
                        if (!email.equals(responseUserInfo.body.email))
                            Commons.setEMail(mRegisterMember, responseUserInfo.body.email);
                    }

                    // 전역 변수로 유저 정보를 기억함.
                    BizLogics.setResponseUserInfo(mRegisterMember, responseUserInfo);

                    mRegisterMember.onUpdateMemberSuccess();
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseUserInfo> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());
                mRegisterMember.onUpdateMemberFail(t.getMessage());
            }
        });

    }


}
