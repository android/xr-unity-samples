// <copyright file="CameraCapturePlugin.java" company="Google LLC">
//
// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
// ----------------------------------------------------------------------

package com.google.xr.androidxrunitysamples.java;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.ImageFormat;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.CameraCaptureSession;
import android.hardware.camera2.CameraCharacteristics;
import android.hardware.camera2.CameraDevice;
import android.hardware.camera2.CameraManager;
import android.hardware.camera2.CameraMetadata;
import android.hardware.camera2.CaptureFailure;
import android.hardware.camera2.CaptureRequest;
import android.hardware.camera2.TotalCaptureResult;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.Image;
import android.media.ImageReader;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.util.Log;
import android.util.Size;
import android.util.SparseIntArray;
import android.view.Surface;
import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;
import org.json.JSONException;
import org.json.JSONObject;

/**
 * Plugin implementation for camera capture functionality. Provides both single-frame and continuous
 * capture capabilities using Android's Camera2 API. Handles camera initialization, capture
 * sessions, and image processing.
 */
public class CameraCapturePlugin implements IUnityPlugin {

  public static boolean SAVE_TO_FILE_FOR_DEBUG = false;
  public static boolean ENABLE_VERBOSE_LOGGING = false;
  private static final String TAG = "CameraCapturePlugin";
  private static final SparseIntArray ORIENTATIONS = new SparseIntArray();

  static {
    ORIENTATIONS.append(Surface.ROTATION_0, 90);
    ORIENTATIONS.append(Surface.ROTATION_90, 0);
    ORIENTATIONS.append(Surface.ROTATION_180, 270);
    ORIENTATIONS.append(Surface.ROTATION_270, 180);
  }

  private static final String ACTION_CAPTURE_FRAME = "captureSingleFrame";
  private static final String ACTION_START_CONTINUOUS_CAPTURE = "startContinuousCapture";
  private static final String ACTION_STOP_CONTINUOUS_CAPTURE = "stopContinuousCapture";
  private static final String ACTION_SET_VERBOSE_LOGGING = "setVerboseLogging";

  private static final String EVENT_CAPTURE_COMPLETE = "Camera_CaptureComplete";
  private static final String EVENT_CAPTURE_ERROR = "Camera_CaptureError";
  private static final String EVENT_CAMERA_READY = "Camera_Ready";
  private static final String EVENT_FRAME_DATA_AVAILABLE = "Camera_FrameDataAvailable";

  private String currentCameraId;
  private CameraDevice cameraDevice;
  private CameraCaptureSession cameraCaptureSession;
  private ImageReader imageReader;
  private Size imageDimension;
  private Handler backgroundHandler;
  private HandlerThread backgroundThread;
  private Context context;
  private IPluginCallback eventCallback;
  private CameraManager cameraManager;
  private Handler mainThreadHandler;

  private CaptureRequest.Builder captureRequestBuilder;
  private String currentSavePath;
  private volatile boolean isCapturingContinuously = false;

  /** A {@link Semaphore} to prevent the app from exiting before closing the camera. */
  private final Semaphore cameraOpenCloseLock = new Semaphore(1);

  @Override
  public void initialize(Context context, IPluginCallback callback) {
    this.context = context;
    this.eventCallback = callback;
    this.cameraManager = (CameraManager) context.getSystemService(Context.CAMERA_SERVICE);
    this.mainThreadHandler = new Handler(Looper.getMainLooper());
    startBackgroundThread();
    if (ENABLE_VERBOSE_LOGGING) {
      Log.d(TAG, "CameraCapturePlugin initialized.");
    }
    sendSimpleEvent(EVENT_CAMERA_READY);
  }

  @Override
  public void callAction(String actionName, String jsonArgs) {
    if (ENABLE_VERBOSE_LOGGING) {
      Log.d(TAG, "callAction received: " + actionName + " with args: " + jsonArgs);
    }

    if ("setDebugSave".equals(actionName)) {
      try {
        JSONObject args = new JSONObject(jsonArgs);
        SAVE_TO_FILE_FOR_DEBUG = args.getBoolean("enabled");
        Log.i(TAG, "SAVE_TO_FILE_FOR_DEBUG set to: " + SAVE_TO_FILE_FOR_DEBUG);
      } catch (JSONException e) {
        Log.e(TAG, "JSON error setting debug save flag: " + e.getMessage());
        sendErrorEvent("Invalid args for setDebugSave");
      }
      return;
    }

    if (ACTION_SET_VERBOSE_LOGGING.equals(actionName)) {
      try {
        JSONObject args = new JSONObject(jsonArgs);
        ENABLE_VERBOSE_LOGGING = args.getBoolean("enabled");
        Log.i(TAG, "ENABLE_VERBOSE_LOGGING set to: " + ENABLE_VERBOSE_LOGGING);
      } catch (JSONException e) {
        Log.e(TAG, "JSON error setting verbose logging flag: " + e.getMessage());
        sendErrorEvent("Invalid args for setVerboseLogging");
      }
      return;
    }

    if (ACTION_START_CONTINUOUS_CAPTURE.equals(actionName)) {
      if (isCapturingContinuously) {
        sendErrorEvent("Already capturing continuously.");
        return;
      }
      try {
        JSONObject args = new JSONObject(jsonArgs);
        int cameraIndex = args.optInt("cameraIndex", 0);
        int width = args.getInt("width");
        int height = args.getInt("height");
        startContinuousCaptureInternal(cameraIndex, width, height);
      } catch (JSONException e) {
        sendErrorEvent("Invalid arguments for startContinuousCapture: " + e.getMessage());
      }
    } else if (ACTION_STOP_CONTINUOUS_CAPTURE.equals(actionName)) {
      if (!isCapturingContinuously) {
        Log.d(TAG, "Stop continuous capture called, but not currently capturing.");
        return;
      }
      stopContinuousCaptureInternal();
    } else if (ACTION_CAPTURE_FRAME.equals(actionName)) {
      try {
        JSONObject args = new JSONObject(jsonArgs);
        int cameraIndex = args.optInt("cameraIndex", 0);
        int width = args.getInt("width");
        int height = args.getInt("height");
        String savePath = args.getString("savePath");
        this.currentSavePath = savePath;
        captureSingleFrameInternal(cameraIndex, width, height);
      } catch (JSONException e) {
        Log.e(TAG, "JSON parsing error for captureSingleFrame: " + e.getMessage());
        sendErrorEvent("Invalid arguments for captureSingleFrame: " + e.getMessage());
      } catch (Exception e) {
        Log.e(TAG, "Error initiating capture: " + e.getMessage());
        sendErrorEvent("Error initiating capture: " + e.getMessage());
      }
    } else {
      Log.w(TAG, "Unknown action requested: " + actionName);
      sendErrorEvent("Unknown camera action: " + actionName);
    }
  }

  @Override
  public void destroy() {
    if (ENABLE_VERBOSE_LOGGING) {
      Log.d(TAG, "Destroying CameraCapturePluginImpl...");
    }
    stopContinuousCaptureInternal();
    closeCamera();
    stopBackgroundThread();

    if (mainThreadHandler != null) {
      mainThreadHandler.removeCallbacksAndMessages(null);
    }
    this.context = null;
    this.eventCallback = null;
    this.cameraManager = null;
    if (ENABLE_VERBOSE_LOGGING) {
      Log.d(TAG, "CameraCapturePluginImpl destroyed.");
    }
  }

  private void startBackgroundThread() {
    backgroundThread = new HandlerThread("CameraBackground");
    backgroundThread.start();
    backgroundHandler = new Handler(backgroundThread.getLooper());
  }

  private void stopBackgroundThread() {
    if (backgroundThread == null) {
      return;
    }
    backgroundThread.quitSafely();
    try {
      backgroundThread.join(500);
      backgroundThread = null;
      backgroundHandler = null;
    } catch (InterruptedException e) {
      Log.e(TAG, "Interrupted while stopping background thread: " + e.getMessage());
      Thread.currentThread().interrupt();
    }
  }

  private void stopContinuousCaptureInternal() {
    backgroundHandler.post(
        () -> {
          if (ENABLE_VERBOSE_LOGGING) {
            Log.d(TAG, "Stopping continuous capture...");
          }
          try {
            if (cameraCaptureSession != null) {
              cameraCaptureSession.stopRepeating();
            }
          } catch (CameraAccessException e) {
            Log.e(TAG, "Error stopping repeating request: " + e.getMessage());

          } catch (IllegalStateException e) {
            Log.e(
                TAG,
                "Illegal state stopping repeating request (session closed?): " + e.getMessage());

          } finally {
            isCapturingContinuously = false;
            closeCamera();
            if (ENABLE_VERBOSE_LOGGING) {
              Log.d(TAG, "Continuous capture stopped.");
            }
          }
        });
  }

  private String getCameraIdByIndex(int cameraIndex) throws CameraAccessException {
    if (cameraManager == null) {
      throw new IllegalStateException("CameraManager not initialized");
    }
    String[] cameraIds = cameraManager.getCameraIdList();
    if (cameraIds.length == 0) {
      throw new CameraAccessException(
          CameraAccessException.CAMERA_DISCONNECTED, "No cameras available.");
    }
    if (cameraIndex >= 0 && cameraIndex < cameraIds.length) {
      return cameraIds[cameraIndex];
    } else {
      Log.w(TAG, "Invalid camera index " + cameraIndex + ", defaulting to 0.");
      return cameraIds[0];
    }
  }

  private void captureSingleFrameInternal(int cameraIndex, int width, int height) {
    if (context == null || cameraManager == null || backgroundHandler == null) {
      sendErrorEvent("Plugin not properly initialized.");
      return;
    }
    if (cameraDevice != null) {
      Log.w(TAG, "Camera already open, cannot start new capture request now.");
      sendErrorEvent("Camera busy or already open.");
      return;
    }

    backgroundHandler.post(
        () -> {
          if (ActivityCompat.checkSelfPermission(context, Manifest.permission.CAMERA)
              != PackageManager.PERMISSION_GRANTED) {
            Log.e(TAG, "Camera permission not granted!");
            sendErrorEvent("Camera Permission Denied by system.");
            return;
          }

          try {
            currentCameraId = getCameraIdByIndex(cameraIndex);
            CameraCharacteristics characteristics =
                cameraManager.getCameraCharacteristics(currentCameraId);
            StreamConfigurationMap map =
                characteristics.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
            if (map == null) {
              sendErrorEvent("Cannot get stream configuration map.");
              return;
            }

            imageDimension = chooseOptimalSize(map.getOutputSizes(ImageFormat.JPEG), width, height);
            if (ENABLE_VERBOSE_LOGGING) {
              Log.d(
                  TAG,
                  "Selected image dimension: "
                      + imageDimension.getWidth()
                      + "x"
                      + imageDimension.getHeight());
            }

            if (imageReader != null) {
              imageReader.close();
            }
            imageReader =
                ImageReader.newInstance(
                    imageDimension.getWidth(), imageDimension.getHeight(), ImageFormat.JPEG, 1);
            imageReader.setOnImageAvailableListener(onImageAvailableListener, backgroundHandler);

            openCameraDevice();

          } catch (CameraAccessException e) {
            Log.e(TAG, "Camera Access Exception during setup: " + e.getMessage());
            sendErrorEvent("Camera Access Exception: " + e.getMessage());
          } catch (SecurityException e) {
            Log.e(TAG, "Security Exception (likely permission) during setup: " + e.getMessage());
            sendErrorEvent("Camera Permission Denied.");
          } catch (IllegalStateException e) {
            Log.e(TAG, "Illegal State Exception during setup: " + e.getMessage());
            sendErrorEvent("Illegal State during camera setup: " + e.getMessage());
          } catch (Exception e) {
            Log.e(TAG, "Unexpected error during setup: " + e.getMessage(), e);
            sendErrorEvent("Unexpected error during setup: " + e.getMessage());
          }
        });
  }

  private void startContinuousCaptureInternal(int cameraIndex, int width, int height) {
    if (context == null || cameraManager == null || backgroundHandler == null) {
      sendErrorEvent("Plugin not properly initialized.");
      return;
    }

    backgroundHandler.post(
        () -> {
          if (cameraDevice != null) {
            Log.w(TAG, "Request received but camera is already open/busy. Ignoring.");
            sendErrorEvent("Camera busy or already open.");
            return;
          }

          if (ActivityCompat.checkSelfPermission(context, Manifest.permission.CAMERA)
              != PackageManager.PERMISSION_GRANTED) {
            Log.e(TAG, "Camera permission not granted!");
            sendErrorEvent("Camera Permission Denied by system.");
            return;
          }

          try {
            currentCameraId = getCameraIdByIndex(cameraIndex);
            CameraCharacteristics characteristics =
                cameraManager.getCameraCharacteristics(currentCameraId);
            StreamConfigurationMap map =
                characteristics.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
            if (map == null) {
              sendErrorEvent("Cannot get stream configuration map.");
              return;
            }

            imageDimension = chooseOptimalSize(map.getOutputSizes(ImageFormat.JPEG), width, height);
            if (ENABLE_VERBOSE_LOGGING) {
              Log.d(
                  TAG,
                  "[Continuous] Selected image dimension: "
                      + imageDimension.getWidth()
                      + "x"
                      + imageDimension.getHeight());
            }

            if (imageReader != null) {
              imageReader.close();
            }

            int maxImages = 3;
            imageReader =
                ImageReader.newInstance(
                    imageDimension.getWidth(),
                    imageDimension.getHeight(),
                    ImageFormat.JPEG,
                    maxImages);
            imageReader.setOnImageAvailableListener(onImageAvailableListener, backgroundHandler);

            isCapturingContinuously = true;

            openCameraDevice();

          } catch (CameraAccessException e) {
            Log.e(TAG, "[Continuous] Camera Access Exception during setup: " + e.getMessage());
            sendErrorEvent("Camera Access Exception: " + e.getMessage());
            isCapturingContinuously = false;
          } catch (SecurityException e) {
            Log.e(
                TAG,
                "[Continuous] Security Exception (likely permission) during setup: "
                    + e.getMessage());
            sendErrorEvent("Camera Permission Denied.");
            isCapturingContinuously = false;
          } catch (IllegalStateException e) {
            Log.e(TAG, "[Continuous] Illegal State Exception during setup: " + e.getMessage());
            sendErrorEvent("Illegal State during camera setup: " + e.getMessage());
            isCapturingContinuously = false;
          } catch (Exception e) {
            Log.e(TAG, "[Continuous] Unexpected error during setup: " + e.getMessage(), e);
            sendErrorEvent("Unexpected error during setup: " + e.getMessage());
            isCapturingContinuously = false;
          }
        });
  }

  private void openCameraDevice()
      throws CameraAccessException, SecurityException, InterruptedException {
    if (ENABLE_VERBOSE_LOGGING) {
      Log.d(TAG, "Attempting to open camera: " + currentCameraId);
    }
    if (!cameraOpenCloseLock.tryAcquire(2500, TimeUnit.MILLISECONDS)) {
      throw new RuntimeException("Time out waiting to lock camera opening.");
    }

    cameraManager.openCamera(currentCameraId, cameraStateCallback, backgroundHandler);
  }

  private void closeCamera() {
    isCapturingContinuously = false;
    try {
      cameraOpenCloseLock.acquire();
      if (cameraCaptureSession != null) {
        cameraCaptureSession.close();
        cameraCaptureSession = null;
      }
      if (cameraDevice != null) {
        cameraDevice.close();
        cameraDevice = null;
      }
      if (imageReader != null) {
        imageReader.close();
        imageReader = null;
      }
      if (ENABLE_VERBOSE_LOGGING) {
        Log.d(TAG, "Camera closed.");
      }
    } catch (InterruptedException e) {
      Log.e(TAG, "Interrupted while trying to close camera.", e);
      Thread.currentThread().interrupt();
    } finally {
      cameraOpenCloseLock.release();
    }
  }

  private final CameraDevice.StateCallback cameraStateCallback =
      new CameraDevice.StateCallback() {
        @Override
        public void onOpened(@NonNull CameraDevice camera) {
          if (ENABLE_VERBOSE_LOGGING) {
            Log.d(TAG, "CameraDevice onOpened");
          }
          cameraOpenCloseLock.release();
          cameraDevice = camera;
          if (isCapturingContinuously) {
            createContinuousCaptureSession();
          } else {
            createCaptureSession();
          }
        }

        @Override
        public void onDisconnected(@NonNull CameraDevice camera) {
          Log.w(TAG, "CameraDevice onDisconnected");
          cameraOpenCloseLock.release();
          closeCamera();
        }

        @Override
        public void onError(@NonNull CameraDevice camera, int error) {
          Log.e(TAG, "CameraDevice onError: " + error);
          cameraOpenCloseLock.release();
          closeCamera();
          sendErrorEvent("Camera device error: " + error);
        }
      };

  private void createCaptureSession() {
    if (cameraDevice == null || imageReader == null || backgroundHandler == null) {
      Log.e(TAG, "Cannot create capture session, dependencies missing.");
      sendErrorEvent("Internal error creating capture session.");
      return;
    }
    try {
      List<Surface> outputSurfaces = Collections.singletonList(imageReader.getSurface());

      CaptureRequest.Builder sessionRequestBuilder =
          cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_STILL_CAPTURE);
      sessionRequestBuilder.addTarget(imageReader.getSurface());

      cameraDevice.createCaptureSession(
          outputSurfaces,
          new CameraCaptureSession.StateCallback() {
            @Override
            public void onConfigured(@NonNull CameraCaptureSession session) {
              if (ENABLE_VERBOSE_LOGGING) {
                Log.d(TAG, "CameraCaptureSession onConfigured");
              }
              if (cameraDevice == null) {
                Log.w(TAG, "Camera closed before session configured.");
                session.close();
                return;
              }
              cameraCaptureSession = session;
              triggerCapture();
            }

            @Override
            public void onConfigureFailed(@NonNull CameraCaptureSession session) {
              Log.e(TAG, "CameraCaptureSession onConfigureFailed");
              sendErrorEvent("Failed to configure camera capture session.");
              closeCamera();
            }
          },
          backgroundHandler);

    } catch (CameraAccessException e) {
      Log.e(TAG, "Camera Access Exception during createCaptureSession: " + e.getMessage());
      sendErrorEvent("Camera Access Exception creating session: " + e.getMessage());
      closeCamera();
    } catch (IllegalStateException e) {
      Log.e(TAG, "Illegal State Exception during createCaptureSession: " + e.getMessage());
      sendErrorEvent("Illegal state creating session: " + e.getMessage());
      closeCamera();
    }
  }

  private void createContinuousCaptureSession() {

    try {
      List<Surface> outputSurfaces = Collections.singletonList(imageReader.getSurface());

      final CaptureRequest.Builder captureRequestBuilder =
          cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_PREVIEW);
      captureRequestBuilder.addTarget(imageReader.getSurface());

      captureRequestBuilder.set(CaptureRequest.CONTROL_MODE, CameraMetadata.CONTROL_MODE_AUTO);
      captureRequestBuilder.set(
          CaptureRequest.CONTROL_AF_MODE, CaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
      captureRequestBuilder.set(
          CaptureRequest.CONTROL_AE_MODE, CaptureRequest.CONTROL_AE_MODE_ON_AUTO_FLASH);
      captureRequestBuilder.set(CaptureRequest.JPEG_ORIENTATION, 90);

      cameraDevice.createCaptureSession(
          outputSurfaces,
          new CameraCaptureSession.StateCallback() {
            @Override
            public void onConfigured(@NonNull CameraCaptureSession session) {
              if (ENABLE_VERBOSE_LOGGING) {
                Log.d(TAG, "Continuous Capture Session onConfigured");
              }
              if (cameraDevice == null) {
                return;
              }

              cameraCaptureSession = session;
              try {

                CaptureRequest repeatingRequest = captureRequestBuilder.build();
                cameraCaptureSession.setRepeatingRequest(repeatingRequest, null, backgroundHandler);
                isCapturingContinuously = true;
                if (ENABLE_VERBOSE_LOGGING) {
                  Log.i(TAG, "Continuous capture started.");
                }

              } catch (CameraAccessException e) {
                Log.e(TAG, "Failed to start repeating request: " + e.getMessage());
                sendErrorEvent("Failed to start continuous capture: " + e.getMessage());
                isCapturingContinuously = false;
                closeCamera();
              } catch (IllegalStateException e) {
                Log.e(TAG, "Illegal state starting repeating request: " + e.getMessage());
                sendErrorEvent("Illegal state starting capture: " + e.getMessage());
                isCapturingContinuously = false;
                closeCamera();
              }
            }

            @Override
            public void onConfigureFailed(@NonNull CameraCaptureSession session) {
              Log.e(TAG, "Continuous Capture Session onConfigureFailed");
              sendErrorEvent("Failed to configure continuous capture session.");
              closeCamera();
            }
          },
          backgroundHandler);

    } catch (CameraAccessException | IllegalStateException e) {
      Log.e(TAG, "Error creating continuous capture session: " + e.getMessage());
      sendErrorEvent("Error creating capture session: " + e.getMessage());
      closeCamera();
    }
  }

  private void triggerCapture() {
    if (cameraDevice == null || cameraCaptureSession == null || backgroundHandler == null) {
      Log.e(TAG, "Cannot trigger capture, dependencies missing.");
      sendErrorEvent("Internal error triggering capture.");
      return;
    }
    try {
      if (ENABLE_VERBOSE_LOGGING) {
        Log.d(TAG, "Triggering single image capture.");
      }
      final CaptureRequest.Builder captureBuilder =
          cameraDevice.createCaptureRequest(CameraDevice.TEMPLATE_STILL_CAPTURE);
      captureBuilder.addTarget(imageReader.getSurface());

      captureBuilder.set(CaptureRequest.CONTROL_MODE, CameraMetadata.CONTROL_MODE_AUTO);

      captureBuilder.set(CaptureRequest.JPEG_ORIENTATION, 90);

      CameraCaptureSession.CaptureCallback captureCallback =
          new CameraCaptureSession.CaptureCallback() {
            @Override
            public void onCaptureCompleted(
                @NonNull CameraCaptureSession session,
                @NonNull CaptureRequest request,
                @NonNull TotalCaptureResult result) {
              super.onCaptureCompleted(session, request, result);
              if (ENABLE_VERBOSE_LOGGING) {
                Log.d(TAG, "Capture attempt completed.");
              }
            }

            @Override
            public void onCaptureFailed(
                @NonNull CameraCaptureSession session,
                @NonNull CaptureRequest request,
                @NonNull CaptureFailure failure) {
              super.onCaptureFailed(session, request, failure);
              Log.e(TAG, "Capture attempt failed. Reason: " + failure.getReason());
              sendErrorEvent("Capture failed. Reason: " + failure.getReason());
            }
          };

      cameraCaptureSession.stopRepeating();
      cameraCaptureSession.abortCaptures();
      cameraCaptureSession.capture(captureBuilder.build(), captureCallback, backgroundHandler);

    } catch (CameraAccessException e) {
      Log.e(TAG, "Camera Access Exception during triggerCapture: " + e.getMessage());
      sendErrorEvent("Camera Access Exception triggering capture: " + e.getMessage());
      closeCamera();
    } catch (IllegalStateException e) {
      Log.e(TAG, "Illegal State Exception during triggerCapture: " + e.getMessage());
      sendErrorEvent("Illegal state triggering capture: " + e.getMessage());
      closeCamera();
    }
  }

  private final ImageReader.OnImageAvailableListener onImageAvailableListener =
      reader -> {
        if (ENABLE_VERBOSE_LOGGING) {
          Log.d(TAG, "Image available!");
        }
        Image image = null;

        ByteBuffer directBuffer = null;

        if (currentSavePath == null && SAVE_TO_FILE_FOR_DEBUG) {
          Log.e(
              TAG,
              "Save path is null or empty in onImageAvailable while SAVE_TO_FILE_FOR_DEBUG is"
                  + " true.");
          sendErrorEvent("Internal error: Save path not set for image debug save.");
          reader.acquireLatestImage().close();
          return;
        }

        byte[] bytes;
        try {
          if (isCapturingContinuously) {

            image = reader.acquireLatestImage();
          } else {
            image = reader.acquireLatestImage();
          }

          if (image == null) {
            if (ENABLE_VERBOSE_LOGGING) {
              Log.v(TAG, "No image available yet.");
            }
            return;
          }

          if (image.getFormat() != ImageFormat.JPEG) {
            Log.e(TAG, "Image format is not JPEG!");
            sendErrorEvent("Internal error: Unexpected image format.");
            return;
          }

          ByteBuffer buffer = image.getPlanes()[0].getBuffer();
          bytes = new byte[buffer.remaining()];
          buffer.get(bytes);

          if (SAVE_TO_FILE_FOR_DEBUG && currentSavePath != null & !isCapturingContinuously) {
            saveImageInternal(bytes, currentSavePath);
          }

          directBuffer = ByteBuffer.allocateDirect(bytes.length);
          directBuffer.put(bytes);
          directBuffer.flip();

          long imageDataPtr = getDirectBufferAddress(directBuffer);

          if (imageDataPtr != 0) {
            if (ENABLE_VERBOSE_LOGGING) {
              Log.i(
                  TAG,
                  "Sending frame data event to Unity. Pointer: "
                      + imageDataPtr
                      + ", Size: "
                      + bytes.length);
            }
            sendFrameDataEvent(
                imageDataPtr, bytes.length, image.getWidth(), image.getHeight(), "JPEG");

            final ByteBuffer bufferToRelease = directBuffer;
            final long ptrToRelease = imageDataPtr;
            mainThreadHandler.postDelayed(
                () -> {
                  if (ENABLE_VERBOSE_LOGGING) {
                    Log.d(TAG, "Releasing direct buffer (Pointer: " + ptrToRelease + ")");
                  }
                },
                500);

          } else {
            Log.e(TAG, "Failed to get direct buffer address!");
            sendErrorEvent("Internal error preparing image data buffer.");
          }

        } catch (IOException e) {
          Log.e(TAG, "IOException during image processing: " + e.getMessage());
          sendErrorEvent("IOException processing image: " + e.getMessage());
        } catch (IllegalStateException e) {
          Log.e(
              TAG, "IllegalStateException in onImageAvailable (reader closed?): " + e.getMessage());
          sendErrorEvent("Internal error processing image: " + e.getMessage());
        } catch (Exception e) {
          Log.e(TAG, "Unexpected error in onImageAvailable: " + e.getMessage(), e);
          sendErrorEvent("Unexpected error processing image: " + e.getMessage());
        } finally {

          bytes = null;

          if (image != null) {
            image.close();
          }

          if (!isCapturingContinuously) {
            closeCamera();
          }
        }
      };

  private void saveImageInternal(byte[] bytes, String savePath) throws IOException {

    File outputFile = null;
    FileOutputStream outputStream = null;
    try {
      outputFile = new File(savePath);
      File outputDir = outputFile.getParentFile();
      if (outputDir != null && !outputDir.exists()) {
        if (!outputDir.mkdirs()) {
          Log.e(TAG, "Failed to create directory: " + outputDir.getAbsolutePath());

          return;
        }
      }
      outputStream = new FileOutputStream(outputFile);
      outputStream.write(bytes);
      if (ENABLE_VERBOSE_LOGGING) {
        Log.i(TAG, "[Debug] Image Saved to: " + outputFile.getAbsolutePath());
      }

    } finally {
      if (outputStream != null) {
        try {
          outputStream.close();
        } catch (IOException e) {
          /* ignore close error */
        }
      }
    }
  }

  private void sendEvent(String jsonPayload) {
    if (eventCallback != null) {
      if (ENABLE_VERBOSE_LOGGING) {
        Log.d(TAG, "Sending event: " + jsonPayload);
      }
      try {
        eventCallback.OnEvent(jsonPayload);
      } catch (Exception e) {
        Log.e(TAG, "Exception sending event to Unity: " + e.getMessage());
      }
    } else {
      Log.w(TAG, "Cannot send event, callback is null.");
    }
  }

  private void sendFrameDataEvent(
      long imageDataPtr, int imageSize, int width, int height, String format) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_FRAME_DATA_AVAILABLE);
      data.put("ImageDataPtr", imageDataPtr);
      data.put("ImageSize", imageSize);
      data.put("Width", width);
      data.put("Height", height);
      data.put("Format", format);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating frame data event: " + e.getMessage());
    }
  }

  private void sendSimpleEvent(String eventName) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", eventName);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating simple event (" + eventName + "): " + e.getMessage());
    }
  }

  private void sendCaptureCompleteEvent(String imagePath) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_CAPTURE_COMPLETE);
      data.put("ImagePath", imagePath);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating capture complete event: " + e.getMessage());
    }
  }

  private void sendErrorEvent(String errorMessage) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_CAPTURE_ERROR);
      data.put("Error", errorMessage);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating error event: " + e.getMessage());
    }
  }

  private static Size chooseOptimalSize(
      Size[] choices, int textureViewWidth, int textureViewHeight) {

    List<Size> bigEnough = new ArrayList<>();

    List<Size> notBigEnough = new ArrayList<>();
    int w = textureViewWidth;
    int h = textureViewHeight;
    for (Size option : choices) {
      if (option.getWidth() >= w && option.getHeight() >= h) {

        if (Math.abs((double) option.getWidth() / option.getHeight() - (double) w / h) < 0.05) {
          bigEnough.add(option);
        }
      } else {
        notBigEnough.add(option);
      }
    }

    if (bigEnough.size() > 0) {
      if (ENABLE_VERBOSE_LOGGING) {
        Log.d(TAG, "Found suitable sizes >= requested. Smallest is best.");
      }
      return Collections.min(
          bigEnough,
          (s1, s2) ->
              Long.signum(
                  (long) s1.getWidth() * s1.getHeight() - (long) s2.getWidth() * s2.getHeight()));
    } else if (notBigEnough.size() > 0) {
      Log.w(TAG, "No size >= requested found. Picking largest available smaller size.");
      return Collections.max(
          notBigEnough,
          (s1, s2) ->
              Long.signum(
                  (long) s1.getWidth() * s1.getHeight() - (long) s2.getWidth() * s2.getHeight()));
    } else {
      Log.e(TAG, "Couldn't find any suitable preview size");
      return choices[0];
    }
  }

  private long getDirectBufferAddress(ByteBuffer buffer) {
    if (!buffer.isDirect()) {
      return 0;
    }
    try {

      java.lang.reflect.Field addressField = java.nio.Buffer.class.getDeclaredField("address");
      addressField.setAccessible(true);
      return addressField.getLong(buffer);
    } catch (NoSuchFieldException e) {
      Log.e(TAG, "Reflection Error: Cannot find 'address' field in Buffer.", e);
    } catch (IllegalAccessException e) {
      Log.e(TAG, "Reflection Error: Cannot access 'address' field.", e);
    } catch (Exception e) {
      Log.e(TAG, "Reflection Error getting buffer address: " + e.getMessage(), e);
    }

    return 0;
  }
}
