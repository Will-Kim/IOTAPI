package kr.co.hksports.hkrider.rxpresenters;

import android.net.Uri;
import android.os.Environment;
import android.util.Log;

import java.io.File;

import kr.co.hksports.hkrider.member.RegisterBike;
import kr.co.hksports.hkrider.models.ResponseSimple;
import kr.co.hksports.hkrider.rxservices.HKRiderService;
import kr.co.hksports.hkrider.utils.Commons;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Created by WillKim on 06/04/2017.
 */

public class UploadBikeImagePresenter {
    private RegisterBike mRegisterBike;
    private HKRiderService mRiderService;
    private String TAG = "AddRoadBikePresenter";

    public UploadBikeImagePresenter(RegisterBike RegisterBikeActivity) {
        mRegisterBike = RegisterBikeActivity;
        mRiderService = new HKRiderService();
    }

    public void UploadBikeImage(String filePath, String fileName, String extension) {

//        String filePath = Environment.getExternalStorageDirectory().getAbsolutePath()+"/HKRider/"+filename;
        File file = new File(filePath);
        Uri fileUri = Uri.fromFile(file);
        RequestBody requestFile = RequestBody.create(MediaType.parse("image/"+extension), file);
        MultipartBody.Part body = MultipartBody.Part.createFormData("picture", fileName, requestFile);

        if(file.exists()) {
            Call<ResponseSimple> call = mRiderService.getAPIs().postUploadBikeImage(body);

            call.enqueue(new Callback<ResponseSimple>() {

                @Override // 응답 처리.
                public void onResponse(Call<ResponseSimple> call, Response<ResponseSimple> response) {
                    Log.i(TAG, "onResponse");
                    if (response.body().header.result_code != 0) { // Custom 오류 처리.
                    } else { // 정상 결과의 반영.
                        ResponseSimple responseSimple = response.body();

                        if (responseSimple.header.auth_token != null && responseSimple.header.auth_token != "") {
                            String token = Commons.getToken(mRegisterBike);
                            if (!token.equals(responseSimple.header.auth_token))
                                Commons.setToken(mRegisterBike, responseSimple.header.auth_token);
                        }
                        if (responseSimple.header.mailbox_idx > 0) {
                            // 기존의 responseUserInfo 다른지 체크 필요.
                        }

                        mRegisterBike.onImageUploadSuccess();
                    }

                }

                @Override // 오류 처리.
                public void onFailure(Call<ResponseSimple> call, Throwable t) {
                    Log.e(TAG, "onError: " + t.getMessage());

                }
            });

        } else { // 이미지가 없는 경우.
             Commons.showToast(mRegisterBike, "이미지 파일이 존재하지 않습니다.");
        }

    }

}
