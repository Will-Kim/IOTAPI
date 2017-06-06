package kr.co.hksports.hkrider.rxpresenters;

import android.content.Context;
import android.content.DialogInterface;
import android.support.v7.app.AlertDialog;
import android.util.Log;

import kr.co.hksports.hkrider.R;
import kr.co.hksports.hkrider.activities.AvatarMain;
import kr.co.hksports.hkrider.member.BizLogics;
import kr.co.hksports.hkrider.models.PostRidingLog;
import kr.co.hksports.hkrider.models.ResponseUserInfo;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class AddRidingLogPresenter { // 추가와 업데이트를 동시에 진행함.
    private Context ctx;
    private HKRiderService mRiderService;
    private String TAG = "AddRidingLogPresenter";

    public AddRidingLogPresenter(Context context) {
        ctx = context;
        mRiderService = new HKRiderService();
    }

    public void AddRidingLog(final PostRidingLog postRidingLog) {

        if (!Commons.isNetworkAvailable(ctx)) { //인터넷이 안되는 경우에..
            String isPOPUP = Commons.getData(ctx, "ISPOPUP");
            if(!isPOPUP.contains("Y")) {
                Commons.setData(ctx, "ISPOPUP", "Y");
                AlertDialog.Builder builder =
                        new AlertDialog.Builder(ctx, R.style.AppCompatAlertDialogStyle);
                builder.setTitle("네트워크 연결 문제");
                builder.setMessage("주행 기록을 저장하시려면 인터넷 연결 이후에 [OK] 버튼을 클릭하세요. [GPS 정보는 저장하지 않습니다]");
                builder.setPositiveButton("OK", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Commons.setData(ctx, "ISPOPUP", "N");
                        callAddRidingLog(postRidingLog);
                    }
                });
                builder.setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Commons.setData(ctx, "ISPOPUP", "N");
                    }
                });
                builder.show();
            } else {
                Commons.showToast(ctx, "이미 팝업창이 있습니다.");
            }
        } else {
            callAddRidingLog(postRidingLog);
        }

    }

    public void callAddRidingLog(final PostRidingLog postRidingLog) {

        postRidingLog.header.action = "AddRidingLog";
        Call<ResponseUserInfo> call = mRiderService.getAPIs().postAddRidingLog(postRidingLog);

        call.enqueue(new Callback<ResponseUserInfo>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseUserInfo> call, Response<ResponseUserInfo> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
                    Commons.onAddRidingLogFail(ctx);
                } else { // 정상 결과의 반영.
                    ResponseUserInfo responseUserInfo = response.body();

                    Log.i(TAG, "responseUserInfo: " + responseUserInfo.body.email);
                    if (responseUserInfo.header.auth_token != null && responseUserInfo.header.auth_token != "") {
                        String token = Commons.getToken(ctx);
                        if (!token.equals(responseUserInfo.header.auth_token))
                            Commons.setToken(ctx, responseUserInfo.header.auth_token);
                    }
                    if (postRidingLog.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }
                    if (responseUserInfo.body.email != null && responseUserInfo.body.email != "") {
                        String email = Commons.getEMail(ctx);
                        if (!email.equals(responseUserInfo.body.email))
                            Commons.setEMail(ctx, responseUserInfo.body.email);
                    }

                    BizLogics.setResponseUserInfo(ctx, responseUserInfo);

                    Commons.onAddRidingLogSuccess(ctx);
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseUserInfo> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());
                Commons.onAddRidingLogError(ctx);


            }
        });

    }

    private AvatarMain mAvatarMain;
    public void callAddRidingLog(final PostRidingLog postRidingLog, AvatarMain avatarMain, final String fileName) {
        mAvatarMain = avatarMain;

        postRidingLog.header.action = "AddRidingLog";
        Call<ResponseUserInfo> call = mRiderService.getAPIs().postAddRidingLog(postRidingLog);

        call.enqueue(new Callback<ResponseUserInfo>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseUserInfo> call, Response<ResponseUserInfo> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
                    AvatarMain.onAddRidingLogFail(mAvatarMain);
                } else { // 정상 결과의 반영.
                    ResponseUserInfo responseUserInfo = response.body();

                    Log.i(TAG, "responseUserInfo: " + responseUserInfo.body.email);
                    if (responseUserInfo.header.auth_token != null && responseUserInfo.header.auth_token != "") {
                        String token = Commons.getToken(mAvatarMain);
                        if (!token.equals(responseUserInfo.header.auth_token))
                            Commons.setToken(mAvatarMain, responseUserInfo.header.auth_token);
                    }
                    if (postRidingLog.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }
                    if (responseUserInfo.body.email != null && responseUserInfo.body.email != "") {
                        String email = Commons.getEMail(mAvatarMain);
                        if (!email.equals(responseUserInfo.body.email))
                            Commons.setEMail(mAvatarMain, responseUserInfo.body.email);
                    }
                    // 전역 변수로 유저 정보를 기억함.
                    BizLogics.setResponseUserInfo(mAvatarMain, responseUserInfo);

                    AvatarMain.onAddRidingLogSuccess(mAvatarMain, fileName);
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseUserInfo> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());
                AvatarMain.onAddRidingLogError(mAvatarMain);


            }
        });

    }

}
