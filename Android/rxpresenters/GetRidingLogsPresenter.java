package kr.co.hksports.hkrider.rxpresenters;

import android.content.Context;
import android.content.DialogInterface;
import android.support.v7.app.AlertDialog;
import android.util.Log;

import kr.co.hksports.hkrider.R;
import kr.co.hksports.hkrider.fragments.AvatarTabRecordsFrag;
import kr.co.hksports.hkrider.member.BizLogics;
import kr.co.hksports.hkrider.models.PostGetRidingLogs;
import kr.co.hksports.hkrider.models.ResponseRidingLogs;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class GetRidingLogsPresenter { // 추가와 업데이트를 동시에 진행함.
    private Context ctx;
    private AvatarTabRecordsFrag mTabRecords;
    private HKRiderService mRiderService;
    private String TAG = "GetRidingLogsPresenter";

    public GetRidingLogsPresenter(AvatarTabRecordsFrag tabRecords) {
        ctx = tabRecords.getContext();
        mTabRecords = tabRecords;
        mRiderService = new HKRiderService();
    }

    public void GetRidingLogs(final PostGetRidingLogs postGetRidingLogs) {

        if (!Commons.isNetworkAvailable(mTabRecords.getContext())) { //인터넷이 안되는 경우에..
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
                        callGetRidingLogs(postGetRidingLogs);
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
            callGetRidingLogs(postGetRidingLogs);
        }

    }

    public void callGetRidingLogs(final PostGetRidingLogs postGetRidingLogs) {

        postGetRidingLogs.header.action = "GetRidingLogs";
        Call<ResponseRidingLogs> call = mRiderService.getAPIs().postGetRidingLog(postGetRidingLogs);

        call.enqueue(new Callback<ResponseRidingLogs>() {

            @Override // 응답 처리.
            public void onResponse(Call<ResponseRidingLogs> call, Response<ResponseRidingLogs> response) {
                Log.i(TAG, "onResponse");
                if(response.body().header.result_code != 0) { // Custom 오류 처리.
//                    Commons.showToast(mRegisterBike, response.body().header.message);
                    mTabRecords.onResponseRidingLogsFail(response.body().header.message);
                } else { // 정상 결과의 반영.
                    ResponseRidingLogs responseRidingLogs = response.body();

                    Log.i(TAG, "SIZE responseRidingLogs: " + responseRidingLogs.body.size());
                    if (responseRidingLogs.header.auth_token != null && responseRidingLogs.header.auth_token != "") {
                        String token = Commons.getToken(mTabRecords.getContext());
                        if (!token.equals(responseRidingLogs.header.auth_token))
                            Commons.setToken(mTabRecords.getContext(), responseRidingLogs.header.auth_token);
                    }
                    if (responseRidingLogs.header.mailbox_idx > 0) {
                        // 기존의 responseUserInfo 다른지 체크 필요.
                    }


                    // 전역 변수로 유저 정보를 기억함.
                    BizLogics.setResponseRidingLogs(responseRidingLogs);

                    mTabRecords.onResponseRidingLogsSuccess();
                }

            }

            @Override // 오류 처리.
            public void onFailure(Call<ResponseRidingLogs> call, Throwable t) {
                Log.e(TAG, "onError: " + t.getMessage());
                mTabRecords.onResponseRidingLogsError(t.getMessage());


            }
        });

    }

}
