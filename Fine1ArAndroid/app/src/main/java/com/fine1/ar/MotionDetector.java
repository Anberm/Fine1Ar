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

    private static final String TAG = "MotionDetector";
    static final int NON_SWING = -1;
    static final int LEFT_SWING = 0;
    static final int RIGHT_SWING = 1;
    static final int UP_SWING = 2;
    static final int DOWN_SWING = 3;

    private static final int DETECTION_INTERVAL = 100;     // 1000 ms
    private static final float DETECTION_VALUE = 0.3f;      // 1.7 rad/s


    private boolean mIsEnable;

    private boolean oIsEnable = true;
    private static final int O_DETECTION_INTERVAL = 1000;     // 1000 ms

    private SensorManager mSensorManager;
    private boolean mIsStartSensor;
    private IMotionDetectListener mListener;

    private final Handler mHandler;

    // 速度阈值，当摇晃速度达到这值后产生作用
    private static final int SPEED_SHRESHOLD = 200;
    // 两次检测的时间间隔
    private static final int UPTATE_INTERVAL_TIME = 100;
    private float lastX;
    private float lastY;
    private float lastZ;
    private long lastUpdateTime;

    //初始化数组
    private float[] gravity = new float[3];//用来保存加速度传感器的值
    private float[] r = new float[9];//
    private float[] geomagnetic = new float[3];//用来保存地磁传感器的值
    private float[] values = new float[3];//用来保存最终的结果


    interface IMotionDetectListener {
        void onMotion(int direction);

        void onChanged(float gx, float gy, float gz);

        void onShake();

        void onAngleChanged(double gx, double gy, double gz);
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
                if(oIsEnable){
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
                    Log.d(TAG,"速度："+speed);
                    if (speed >= SPEED_SHRESHOLD) {
                        oIsEnable=false;
                        Log.d(TAG,"速度 Shake："+speed);
                        mListener.onShake();
                        startOIntervalTimer();
                    }
                }


            }
        }

//        if (event.sensor.getType() == Sensor.TYPE_MAGNETIC_FIELD) {
//            geomagnetic = event.values;
//        }
//        if (event.sensor.getType() == Sensor.TYPE_ACCELEROMETER) {
//
//            if (mListener != null) {
//                if (oIsEnable) {
//                    if (mIsEnable) {
//                        gravity = event.values;
//                        double[] oris= getOritation();
//                        mListener.onAngleChanged(oris[0],oris[1],oris[2]);
//                        mIsEnable = false;
//                        startIntervalTimer();
//                    }
//                }
//            }
//        }
    }

    /**
     * 获取手机旋转角度
     */
    public double[] getOritation() {
        // r从这里返回
        SensorManager.getRotationMatrix(r, null, gravity, geomagnetic);
        //values从这里返回
        SensorManager.getOrientation(r, values);
        //提取数据
        double degreeZ = Math.toDegrees(values[0]);
        double degreeX = Math.toDegrees(values[1]);
        double degreeY = Math.toDegrees(values[2]);
        Log.d("Oritation", " " + degreeX + " " + degreeY + " " + degreeZ);
        return new double[]{
                degreeX,
                degreeY,
                degreeZ
        };

    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    void startSensor() {
        if (!mIsStartSensor) {
            if (mSensorManager != null) {
                Sensor gyro = mSensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR);
                Sensor magneticSensor = mSensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD);
                Sensor accelerometerSensor = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
                if (gyro != null) {
                    mIsStartSensor = mSensorManager.registerListener(this, gyro, SensorManager.SENSOR_DELAY_NORMAL);
                    mSensorManager.registerListener(this, magneticSensor, SensorManager.SENSOR_DELAY_NORMAL);
                    mSensorManager.registerListener(this, accelerometerSensor, SensorManager.SENSOR_DELAY_NORMAL);
                }
                gravity = new float[3];//用来保存加速度传感器的值
                r = new float[9];//
                geomagnetic = new float[3];//用来保存地磁传感器的值
                values = new float[3];//用来保存最终的结果
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
        } else if (sensorValues[0] > (DETECTION_VALUE)) {
            direction = UP_SWING;
        } else if (sensorValues[0] < (DETECTION_VALUE * -1)) {
            direction = DOWN_SWING;
        } else {
            direction = NON_SWING;
        }
        return direction;
    }

    private void startIntervalTimer() {
        if (mHandler != null) {
            mHandler.postDelayed(mTimeout, DETECTION_INTERVAL);
        }
    }

    private void startOIntervalTimer() {
        if (mHandler != null) {
            mHandler.postDelayed(oTimeout, O_DETECTION_INTERVAL);
        }
    }


    private final Runnable mTimeout = new Runnable() {
        @Override
        public void run() {
            mIsEnable = true;
        }
    };

    private final Runnable oTimeout = new Runnable() {
        @Override
        public void run() {
            oIsEnable = true;
        }
    };
}
