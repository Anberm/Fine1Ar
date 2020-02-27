package com.fine1.ar;


import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Handler;

class MotionDetector implements SensorEventListener {

    static final int NON_SWING = -1;
    static final int LEFT_SWING = 0;
    static final int RIGHT_SWING = 1;
    static final int UP_SWING = 2;
    static final int DOWN_SWING = 3;

    private static final int DETECTION_INTERVAL = 200;     // 1000 ms
    private static final float DETECTION_VALUE = 0.3f;      // 1.7 rad/s


    private boolean mIsEnable;

    private SensorManager mSensorManager;
    private boolean mIsStartSensor;
    private IMotionDetectListener mListener;

    private final Handler mHandler;


    interface IMotionDetectListener {
        void onMotion(int direction);

        void onChanged(float gx, float gy, float gz);
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
                mListener.onChanged(event.values[0], event.values[1], event.values[2]);

                if (mIsEnable) {
                    int direction = judgeDirection(event.values);
                        mListener.onMotion(direction);
                        mIsEnable = false;
                        startIntervalTimer();
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
                    mIsStartSensor = mSensorManager.registerListener(this, gyro, SensorManager.SENSOR_DELAY_FASTEST);
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
