package com.fine1.ar;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import org.apache.http.params.HttpParams;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.TimeUnit;

import wendu.dsbridge.DWebView;
import wendu.dsbridge.OnReturnValue;

public class MainActivity extends AppCompatActivity implements MotionDetector.IMotionDetectListener {

    private MotionDetector mMotionDetector;

    private TextView mTextViewX;
    private TextView mTextViewY;
    private TextView mTextViewZ;
    private TextView textViewLabelGyroX;
    private TextView textViewLabelGyroY;
    private TextView textViewLabelGyroZ;

    private  TextView textViewMotion;
    private DWebView dWebView;

    public final static String ApiIp ="192.168.168.125"; //"http://admin:admin@192.168.0.51/LAPI/V1.0/Channel/0/PTZ/PTZCtrl";
    public final static String VideoIp ="192.168.168.13";//192.168.0.51"; //"http://admin:admin@192.168.0.51/LAPI/V1.0/Channel/0/PTZ/PTZCtrl";


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mTextViewX = (TextView) findViewById(R.id.textViewGyroX);
        mTextViewY = (TextView) findViewById(R.id.textViewGyroY);
        mTextViewZ = (TextView) findViewById(R.id.textViewGyroZ);

        textViewLabelGyroX = (TextView) findViewById(R.id.textViewLabelGyroX);
        textViewLabelGyroY = (TextView) findViewById(R.id.textViewLabelGyroY);
        textViewLabelGyroZ = (TextView) findViewById(R.id.textViewLabelGyroZ);
        textViewMotion = (TextView) findViewById(R.id.textViewMotion);

        mTextViewX.setVisibility(View.INVISIBLE);
        mTextViewY.setVisibility(View.INVISIBLE);
        mTextViewZ.setVisibility(View.INVISIBLE);

        textViewLabelGyroX.setVisibility(View.INVISIBLE);
        textViewLabelGyroY.setVisibility(View.INVISIBLE);
        textViewLabelGyroZ.setVisibility(View.INVISIBLE);

        DWebView.setWebContentsDebuggingEnabled(true);
        dWebView = (DWebView) findViewById(R.id.webview);
        dWebView.loadUrl("http://"+ApiIp+":8000/webrtcstreamer.html?video=rtsp://admin:admin@"+VideoIp+"/media/video1&options=rtptransport=tcp&timeout=60&");
//        dWebView.loadUrl("http://192.168.0.100:8000/webrtcstreamer.html?video=rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov&options=rtptransport=tcp&timeout=60&");
        mMotionDetector = new MotionDetector(this);
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (mMotionDetector != null) {
            mMotionDetector.startSensor();
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (mMotionDetector != null) {
            mMotionDetector.stopSensor();
        }
    }

    @Override
    public void onMotion(int direction) {

        if (direction == MotionDetector.LEFT_SWING) {
            textViewMotion.setText("LEFT");

        } else if (direction == MotionDetector.RIGHT_SWING) {
            textViewMotion.setText("RIGHT");

        } else if (direction == MotionDetector.UP_SWING) {
            textViewMotion.setText("UP");

        } else if (direction == MotionDetector.DOWN_SWING) {
            textViewMotion.setText("DOWN");

        } else {
            // do nothing
             textViewMotion.setText("STOP");
        }
        callWeb(direction);
    }

    @Override
    public void onChanged(float gx, float gy, float gz) {
        mTextViewX.setText(String.valueOf(gx));
        mTextViewY.setText(String.valueOf(gy));
        mTextViewZ.setText(String.valueOf(gz));
    }

    private  void callWeb(int direction){
        dWebView.callHandler("motionChange", new Object[]{direction,VideoIp}, new OnReturnValue<String>() {
            @Override
            public void onValue(String retValue) {
//                showToast(retValue);
            }
        });
    }
    void showToast(Object o) {
        Toast.makeText(this, o.toString(), Toast.LENGTH_SHORT).show();
    }
}

