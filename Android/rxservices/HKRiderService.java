package kr.co.hksports.hkrider.rxservices;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Created by WillKim on 06/04/2017.
 */

public class HKRiderService {
    private static final String HKRIDER_SERVER_URL = "http://...";
    private HKRiderAPIs mHKRiderAPIs;

    public HKRiderService() {
        Retrofit.Builder builder = new Retrofit.Builder()
                .baseUrl(HKRIDER_SERVER_URL)
                .client(createOkHttpClient())
                .addConverterFactory(GsonConverterFactory.create());

        Retrofit retrofit = builder.build();
        mHKRiderAPIs = retrofit.create(HKRiderAPIs.class);
    }

    private static OkHttpClient createOkHttpClient() {
        OkHttpClient.Builder builder = new OkHttpClient.Builder();
        HttpLoggingInterceptor interceptor = new HttpLoggingInterceptor();
        interceptor.setLevel(HttpLoggingInterceptor.Level.BODY);
        builder.addInterceptor(interceptor);
        return builder.build();
    }

    public HKRiderAPIs getAPIs() {
        return mHKRiderAPIs;
    }

}
