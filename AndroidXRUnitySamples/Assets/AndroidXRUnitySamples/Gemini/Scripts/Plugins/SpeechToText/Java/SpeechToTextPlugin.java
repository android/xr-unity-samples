// <copyright file="SpeechToTextPlugin.java" company="Google LLC">
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
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.util.Log;
import androidx.core.app.ActivityCompat;
import com.unity3d.player.UnityPlayer;
import java.util.ArrayList;
import org.json.JSONException;
import org.json.JSONObject;

/** Implements STT functionality for Unity objects. */
public class SpeechToTextPlugin implements IUnityPlugin {
  private static final String TAG = "STTPlugin";
  private static final String ACTION_START_STT = "startSpeechToText";

  private static final String EVENT_RESULT = "STT_Result";
  private static final String EVENT_ERROR = "STT_Error";
  private static final String EVENT_READY = "STT_Ready";
  private static final String EVENT_BEGINNING = "STT_Beginning";
  private static final String EVENT_END = "STT_End";

  private SpeechRecognizer speechRecognizer;
  private Intent speechRecognizerIntent;
  private Context context;
  private IPluginCallback eventCallback;

  @Override
  public void initialize(Context context, IPluginCallback callback) {
    this.context = context;
    this.eventCallback = callback;
    Log.d(TAG, "SpeechToTextPluginImpl initialized.");

    speechRecognizerIntent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
    speechRecognizerIntent.putExtra(
        RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
    speechRecognizerIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, "en-US");
    speechRecognizerIntent.putExtra(RecognizerIntent.EXTRA_PROMPT, "Speak now");
  }

  @Override
  public void callAction(String actionName, String jsonArgs) {
    Log.d(TAG, "callAction received: " + actionName + " with args: " + jsonArgs);

    UnityPlayer.currentActivity.runOnUiThread(
        () -> {
          if (ACTION_START_STT.equals(actionName)) {
            startSpeechToTextInternal();
          } else {
            Log.w(TAG, "Unknown action requested: " + actionName);
            sendErrorEvent("Unknown action: " + actionName, -1);
          }
        });
  }

  @Override
  public void destroy() {
    Log.d(TAG, "Destroying SpeechToTextPluginImpl...");

    UnityPlayer.currentActivity.runOnUiThread(
        () -> {
          if (speechRecognizer != null) {
            speechRecognizer.stopListening();
            speechRecognizer.destroy();
            speechRecognizer = null;
            Log.d(TAG, "SpeechRecognizer destroyed.");
          }
        });
    this.context = null;
    this.eventCallback = null;
  }

  private void startSpeechToTextInternal() {
    if (eventCallback == null) {
      Log.e(TAG, "Cannot start STT, callback is null.");
      return;
    }
    if (context == null) {
      Log.e(TAG, "Cannot start STT, context is null.");
      sendErrorEvent("Plugin context not available", -1);
      return;
    }

    if (ActivityCompat.checkSelfPermission(context, Manifest.permission.RECORD_AUDIO)
        != PackageManager.PERMISSION_GRANTED) {
      Log.e(TAG, "Audio recording permission not granted!");
      sendErrorEvent(
          "Audio permission not granted", SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS);

      return;
    }

    if (speechRecognizer != null) {
      speechRecognizer.destroy();
      speechRecognizer = null;
    }

    try {
      speechRecognizer = SpeechRecognizer.createSpeechRecognizer(context);
      if (speechRecognizer == null) {
        Log.e(TAG, "SpeechRecognizer.createSpeechRecognizer returned null");
        sendErrorEvent("Failed to create SpeechRecognizer instance.", -1);
        return;
      }
    } catch (Exception e) {
      Log.e(TAG, "Exception creating SpeechRecognizer: " + e.getMessage());
      sendErrorEvent("Exception creating SpeechRecognizer: " + e.getMessage(), -1);
      return;
    }

    speechRecognizer.setRecognitionListener(recognitionListener);
    speechRecognizer.startListening(speechRecognizerIntent);
    Log.d(TAG, "SpeechRecognizer started listening.");
  }

  private final RecognitionListener recognitionListener =
      new RecognitionListener() {
        @Override
        public void onReadyForSpeech(Bundle bundle) {
          Log.d(TAG, "onReadyForSpeech");
          sendSimpleEvent(EVENT_READY);
        }

        @Override
        public void onBeginningOfSpeech() {
          Log.d(TAG, "onBeginningOfSpeech");
          sendSimpleEvent(EVENT_BEGINNING);
        }

        @Override
        public void onEndOfSpeech() {
          Log.d(TAG, "onEndOfSpeech");
          sendSimpleEvent(EVENT_END);
        }

        @Override
        public void onError(int error) {
          String errorMessage = getErrorText(error);
          Log.e(TAG, "onError: " + errorMessage + " (" + error + ")");
          sendErrorEvent(errorMessage, error);
        }

        @Override
        public void onResults(Bundle results) {
          ArrayList<String> matches =
              results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
          String textResult = "";
          if (matches != null && !matches.isEmpty()) {
            textResult = matches.get(0);
            Log.d(TAG, "onResults: " + textResult);
          } else {
            Log.d(TAG, "onResults: No match found in results bundle.");

            textResult = "";
          }
          sendResultEvent(textResult);
        }

        @Override
        public void onRmsChanged(float rmsdB) {}

        @Override
        public void onBufferReceived(byte[] bytes) {}

        @Override
        public void onPartialResults(Bundle partialResults) {
          Log.d(TAG, "onPartialResults received");
        }

        @Override
        public void onEvent(int eventType, Bundle params) {
          Log.d(TAG, "onEvent received: " + eventType);
        }
      };

  private void sendEvent(String jsonPayload) {
    if (eventCallback != null) {
      Log.d(TAG, "Sending event: " + jsonPayload);

      try {
        eventCallback.OnEvent(jsonPayload);
      } catch (Exception e) {

        Log.e(TAG, "Exception sending event to Unity: " + e.getMessage());
      }
    } else {
      Log.w(TAG, "Cannot send event, callback is null.");
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

  private void sendResultEvent(String text) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_RESULT);
      data.put("Text", text);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating result event: " + e.getMessage());
    }
  }

  private void sendErrorEvent(String errorMessage, int errorCode) {
    JSONObject data = new JSONObject();
    try {
      data.put("Event", EVENT_ERROR);
      data.put("Error", errorMessage);
      data.put("ErrorCode", errorCode);
      data.put("Timestamp", System.currentTimeMillis());
      sendEvent(data.toString());
    } catch (JSONException e) {
      Log.e(TAG, "JSONException creating error event: " + e.getMessage());
    }
  }

  private String getErrorText(int errorCode) {

    String message;
    switch (errorCode) {
      case SpeechRecognizer.ERROR_AUDIO:
        message = "Audio recording error";
        break;
      case SpeechRecognizer.ERROR_CLIENT:
        message = "Client side error";
        break;
      case SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS:
        message = "Insufficient permissions";
        break;
      case SpeechRecognizer.ERROR_NETWORK:
        message = "Network error";
        break;
      case SpeechRecognizer.ERROR_NETWORK_TIMEOUT:
        message = "Network timeout";
        break;
      case SpeechRecognizer.ERROR_NO_MATCH:
        message = "No speech match";
        break;
      case SpeechRecognizer.ERROR_RECOGNIZER_BUSY:
        message = "Recognition service busy";
        break;
      case SpeechRecognizer.ERROR_SERVER:
        message = "Server error";
        break;
      case SpeechRecognizer.ERROR_SPEECH_TIMEOUT:
        message = "No speech input timeout";
        break;
      default:
        message = "Unknown speech error";
        break;
    }
    return message;
  }
}
