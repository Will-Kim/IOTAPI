package kr.co.hksports.hkrider.rxpresenters;

import android.content.Context;
import android.util.Log;

import kr.co.hksports.hkrider.activities.AvatarInfo;
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

public class GetBikeLostPresenter { // 추가와 업데이트를 동시에 진행함.
    private Context ctx;
    private AvatarInfo mAvatarInfo;
    private HKRiderService mRiderService;
    private String TAG = "GetBikeLostPresenter";

    public GetBikeLostPresenter(AvatarInfo AvatarInfoActivity) {
        mAvatarInfo = AvatarInfoActivity;
        ctx = AvatarInfoActivity;
        mRiderService = new HKRiderService();
    }

    public void GetBikeLost(final PostBikeLost postBikeLost) {

        postBikeLost.header.action = "GetBikeLost";
        Call<PostBikeLost> call = mRiderService.getAPIs().postGetBikeLost(postBikeLost);

        call.enqueue(new Callback<PostBikeLost>() {

            @Override // 응답 처리.
            public void onResponse(Call<PostBikeLost> call, Response<PostBikeLost> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
                    mAvatarInfo.onGetBikeLostError();
                } else { // 정상 결과의 반영.
                    PostBikeLost postBikeLost = response.body();

                    Log.i(TAG, "responseUserInfo: " + postBikeLost.body.nickname);
                    if (postBikeLost.header.auth_token != null && postBikeLost.header.auth_token != "") {
                        String token = Commons.getToken(ctx);
                        if (!token.equals(postBikeLost.header.auth_token))
                            Commons.setToken(ctx, postBikeLost.header.auth_token);
                    }
                    if (postBikeLost.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }

                    mAvatarInfo.onGetBikeLostSuccess(postBikeLost.body);
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<PostBikeLost> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());

                mAvatarInfo.onGetBikeLostFailure();

            }
        });

    }

}
