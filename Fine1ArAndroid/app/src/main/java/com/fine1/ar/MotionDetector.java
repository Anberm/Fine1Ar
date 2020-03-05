package com.fine1.ar;


import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.nfc.Tag;
import android.os.Handler;
import android.util.Log;

class MotionDetector implements SensorEventListener {

    private  static  final  String TAG="MotionDetector";
    static final int NON_SWING = -1;
    static final int LEFT_SWING = 0;
    static final int RIGHT_SWING = 1;
    static final int UP_SWING = 2;
    static final int DOWN_SWING = 3;

    private static final int DETECTION_INTERVAL = 50;     // 1000 ms
    private static final float DETECTION_VALUE = 0.3f;      // 1.7 rad/s


    private boolean mIsEnable;

    private SensorManager mSensorManager;
    private boolean mIsStartSensor;
    private IMotionDetectListener mListener;

    private final Handler mHandler;

    // 速度阈值，当摇晃速度达到这值后产生作用
    private static final int SPEED_SHRESHOLD = 700;
    // 两次检测的时间间隔
    private static final int UPTATE_INTERVAL_TIME = 50;
    private float lastX;
    private float lastY;
    private float lastZ;
    private long lastUpdateTime;


    interface IMotionDetectListener {
        void onMotion(int direction);

        void onChanged(float gx, float gy, float gz);

        void onShake();
    }

    MotionDetector(Context context) {
        mIsStartSensor = false;

        if (context instanceof IMotionDetectListener) {
            mListener = (IMotionDetectListener) context;
            mHandler = new Handler();
            mSensorManager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
            mIsEnable = false;
        } else {
            throw new ClassCastException("must implement ISwingListener");
        }
    }

    @Override
    public void onSensorChanged(SensorEvent event) {
        if (event.sensor.getType() == Sensor.TYPE_GYROSCOPE) {
            if (mListener != null) {
                if (mIsEnable) {
                    int direction = judgeDirection(event.values);
                    mListener.onMotion(direction);
                    mIsEnable = false;
                    startIntervalTimer();
                }
                mListener.onChanged(event.values[0], event.values[1], event.values[2]);
                // 现在检测时间
                long currentUpdateTime = System.currentTimeMillis();
                // 两次检测的时间间隔
                long timeInterval = currentUpdateTime - lastUpdateTime;
                // 判断是否达到了检测时间间隔
                if (timeInterval < UPTATE_INTERVAL_TIME) return;
                // 现在的时间变成last时间
                lastUpdateTime = currentUpdateTime;
                // 获得x,y,z坐标
                float x = event.values[0];
                float y = event.values[1];
                float z = event.values[2];
                // 获得x,y,z的变化值
                float deltaX = x - lastX;
                float deltaY = y - lastY;
                float deltaZ = z - lastZ;
                // 将现在的坐标变成last坐标
                lastX = x;
                lastY = y;
                lastZ = z;
                double speed = Math.sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ
                        * deltaZ)
                        / timeInterval * 10000;
                // 达到速度阀值，发出提示

                if (speed >= SPEED_SHRESHOLD) {
                    Log.d(TAG,"速度："+speed);
                    mListener.onShake();
                }

            }
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    void startSensor() {
        if (!mIsStartSensor) {
            if (mSensorManager != null) {
                Sensor gyro = mSensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE);
                if (gyro != null) {
                    mIsStartSensor = mSensorManager.registerListener(this, gyro, SensorManager.SENSOR_DELAY_NORMAL);
                }
            }
            if (mIsStartSensor) {
                mIsEnable = mIsStartSensor;
            }
        }
    }

    void stopSensor() {
        if (mIsStartSensor) {
            if (mSensorManager != null) {
                mSensorManager.unregisterListener(this);
                mIsStartSensor = false;
            }
        }
    }

    private int judgeDirection(float[] sensorValues) {
        int direction;

        if (DETECTION_VALUE < sensorValues[1]) {
            direction = LEFT_SWING;
        } else if (sensorValues[1] < (DETECTION_VALUE * -1)) {
            direction = RIGHT_SWING;
        } else if (sensorValues[0] > (DETECTION_VALUE )) {
            direction = UP_SWING;
        }else if (sensorValues[0] < (DETECTION_VALUE * -1)) {
            direction = DOWN_SWING;
        }else {
            direction = NON_SWING;
        }
        return direction;
    }

    private void startIntervalTimer() {
        if (mHandler != null) {
            mHandler.postDelayed(mTimeout, DETECTION_INTERVAL);
        }
    }

    private final Runnable mTimeout = new Runnable() {
        @Override
        public void run() {
            mIsEnable = true;
        }
    };
}
