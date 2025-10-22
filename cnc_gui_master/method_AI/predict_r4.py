import cv2
import os
import numpy as np
import tensorflow as tf
from keras.preprocessing import image
import mysql.connector
import time
import matplotlib
matplotlib.rc('font', family='Microsoft JhengHei')
import matplotlib.pyplot as plt
from datetime import datetime
from flask import Flask, request, jsonify

current_path = os.path.abspath(os.path.dirname(__file__))
os.chdir(current_path)

# input_dir 與 model_path 設定
input_dir = os.path.join(current_path, 'origin')
model_path = os.path.join(current_path, 'r4_model.h5')
model = tf.keras.models.load_model(model_path)
app = Flask(__name__)

# 給主程式所使用的座標點
ps1 = np.array([[152, 196], [648, 244], [548, 300], [496, 344],
                [428, 408], [380, 472], [336, 544], [304, 616], 
                [268, 692], [248, 800], [204, 844]])

ps2 = np.array([[252, 1156], [284, 1276], [316, 1364], [364, 1468],
                [404, 1524], [452, 1596], [492, 1644], [540, 1692],
                [580, 1740], [620, 1772], [668, 1820], [708, 1852],
                [740, 1876], [804, 1924], [860, 1956], [908, 1988],
                [900, 2020], [1052, 2844], [468, 2996], [420, 2996]])

ps3 = np.array([[3088, 1264], [1456, 1568], 
                [1384, 1648], [1360, 1736], 
                [1344, 1832], [1360, 2016], 
                [1408, 2216], [1528, 2576], 
                [1680, 2992], [1968, 2992], 
                [3640, 2224], [3688, 2200]])

ps4 = np.array([[952, 1132], [2032, 1028], 
                [1960, 1164], [1856, 1244], 
                [1752, 1292], [1600, 1332], 
                [1432, 1340], [1184, 1284]])


# 給模型進行預測所使用的座標點
model_ps2 = np.array([[540, 1680], [620, 1760], 
                      [676, 1816], [740, 1864], 
                      [796, 1904], [860, 1936], 
                      [916, 1960], [932, 1976], 
                      [940, 2072], [1076, 2800], 
                      [468, 2992], [396, 2200], 
                      [436, 2080], [516, 2032]])

image_height = 31
image_width = 108

def update_sql_r4(flusher_level_result_value):
    db_connection = mysql.connector.connect(
        host="localhost",  
        user="root",       
        password="ncut2024",  
        database="cnc_db",  
        auth_plugin="mysql_native_password"
    )
    cursor = db_connection.cursor()
    query = """
            UPDATE level_result
            SET flusher_rotary_platform_result = %s
            """
    values = (flusher_level_result_value, )  
    cursor.execute(query, values)
    db_connection.commit()
    print(f"update record id 1 with flusher_level_result_value: {flusher_level_result_value}")
    cursor.close()
    db_connection.close()

def get_roi(image, ps, n):
    x, y, w, h = cv2.boundingRect(ps)
    roi = image[y:y + h, x:x + w]
    print(f'ps = ({x}, {y}), ({x}, {y + h}), ({x + w}, {y + h}), ({x + w}, {y})')
    if n == 1:
        fixed_path = os.path.join(current_path, 'r4')
        if not os.path.exists(fixed_path):
            os.makedirs(fixed_path)
        file_path = os.path.join(fixed_path, 'roi4.png')
        cv2.imwrite(file_path, roi)
    elif n == 2:
        fixed_path = os.path.join(current_path, 'r4')
        if not os.path.exists(fixed_path):
            os.makedirs(fixed_path)
        file_path = os.path.join(fixed_path, 'roi4.png')
        cv2.imwrite(file_path, roi)
    else:
        raise ValueError("n 必須是 1 或 2")
    if n == 1:
        fixed_path = os.path.join(current_path, 'resized_r4')
        if not os.path.exists(fixed_path):
            os.makedirs(fixed_path)
        resized_file_path = os.path.join(fixed_path, 'resized_r4.jpg')
        cv2.imwrite(resized_file_path, roi)
    elif n == 2:
        fixed_path = os.path.join(current_path, 'resized_r4')
        if not os.path.exists(fixed_path):
            os.makedirs(fixed_path)
        resized_file_path = os.path.join(fixed_path, 'resized_r4.jpg')
        cv2.imwrite(resized_file_path, roi)
    return roi

def mask_roi(img, ps):
    x, y, w, h = cv2.boundingRect(ps)
    mask_shape = img.shape[:2]
    mask = np.zeros(mask_shape, dtype=np.uint8)
    cv2.fillPoly(mask, [ps], 255)
    roi = cv2.bitwise_and(img, img, mask=mask)
    roi = roi[y:y+h, x:x+w]
    return roi

# 新增：Sigmoid 非線性對比度映射
def sigmoid_mapping(image, alpha=8, beta=0.5):
    # 將影像歸一化至 [0,1]
    img_norm = image.astype(np.float32) / 255.0
    # 套用 Sigmoid 函數
    mapped = 1 / (1 + np.exp(-alpha * (img_norm - beta)))
    # 轉回 0~255 的範圍
    mapped_uint8 = np.uint8(mapped * 255)
    return mapped_uint8

# 新增：Butterworth 高通濾波器
def butterworth_highpass_filter(image, cutoff, order=3):
    rows, cols = image.shape
    f = np.fft.fft2(np.float32(image))
    fshift = np.fft.fftshift(f)
    crow, ccol = rows // 2, cols // 2
    x = np.arange(0, cols)
    y = np.arange(0, rows)
    x, y = np.meshgrid(x, y)
    D = np.sqrt((x - ccol)**2 + (y - crow)**2)
    D = np.where(D == 0, 1e-5, D)
    H = 1 / (1 + (cutoff / D)**(2 * order))
    fshift_filtered = fshift * H
    f_ishift = np.fft.ifftshift(fshift_filtered)
    img_back = np.fft.ifft2(f_ishift)
    img_back = np.abs(img_back)
    img_back = cv2.normalize(img_back, None, 0, 255, cv2.NORM_MINMAX)
    img_back = img_back.astype(np.uint8)
    return img_back

# 修改後的影像處理流程：使用 Sigmoid、GaussianBlur、Butterworth 高通濾波、Laplacian 邊緣提取
def image_proc(img, ps):
    roi_img = mask_roi(img, ps)
    # 轉換為灰階
    if len(roi_img.shape) == 3:
        gray = cv2.cvtColor(roi_img, cv2.COLOR_BGR2GRAY)
    else:
        gray = roi_img
    # 使用 Sigmoid 映射提升對比度
    mapped = sigmoid_mapping(gray, alpha=7, beta=0.5)
    # 進行高斯平滑 (kernel 大小 3x3)
    smoothed = cv2.GaussianBlur(mapped, (3, 3), 0)
    # 使用 Butterworth 高通濾波器 (cutoff=45, order=3)
    filtered_img = butterworth_highpass_filter(smoothed, cutoff=45, order=3)
    # 使用 Laplacian 提取邊緣
    laplacian = cv2.Laplacian(filtered_img, cv2.CV_64F, ksize=3)
    laplacian = cv2.convertScaleAbs(laplacian)
    return laplacian

# pre_proc 邏輯保持不變，但 image_proc 內部操作已替換
def pre_proc(img):
    roi = get_roi(img, ps4, 1)
    # roi2 = get_roi(img, ps2, 2)
    res = image_proc(img, ps4)
    # res2 = image_proc(img, model_ps2)
    # cv2.imwrite('roi.png', res2)
    return res


def predict(img):
    img = cv2.resize(img, (image_width, image_height))
    img_array = image.img_to_array(img)
    img_array = np.expand_dims(img_array, axis=0)
    img_array /= 255.0  # 標準化處理
    predictions = model.predict(img_array)
    predicted_class = np.argmax(predictions, axis=1)
    predictions = predictions[0]
    predicted_class = predicted_class[0]
    print(f'Predicted probability: {predictions}')
    print(f'Predicted class: {predicted_class}')
    return predicted_class, predictions

def save_predict(image, filename, predicted_class):
    def create_dir(directory):
        if not os.path.exists(directory):
            os.makedirs(directory)

    # 將 current directory 下的 r4_res 作為 base_dir
    base_dir = os.path.join(os.getcwd(), 'r4_res')
    create_dir(base_dir)

    # 創建對應的 predicted_class 的子目錄
    class_dir = os.path.join(base_dir, str(predicted_class))
    create_dir(class_dir)

    # 生成時間戳
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')

    # if np.max(predictions) < 0.8:
    #     base_dir = os.path.join(os.getcwd(), "confusion")
    #     base_dir = os.path.join(base_dir, str(predicted_class))
    # create_dir(base_dir)

    file_path = os.path.join(class_dir, f"photo_{timestamp}.jpg")
    # filename = os.path.join(base_dir, f"{os.path.splitext(filename)[0]}.jpg")
    cv2.imwrite(file_path, image)
    print(f'圖片已保存到: {file_path}')

@app.route('/predict_r4', methods=['GET'])
def load_pic():
    for filename in os.listdir(input_dir):
        if filename.lower().endswith(('.jpg', '.png', 'jpeg')):
            file_path = os.path.join(input_dir, filename)
            img = cv2.imread(file_path, cv2.IMREAD_UNCHANGED)
            if img is None:
                print(f"Can't read the file: {file_path}")
                continue
            r4 = pre_proc(img)
            predicted_class, predictions = predict(r4)
            print(predicted_class)
            update_sql_r4(int(predicted_class))
            save_predict(img, filename, predicted_class)
    return 'Processing complete', 200


def only_flask():
    # deprecated version
    app.run(host='0.0.0.0', port=5004, threaded=True)

def test_version():
    # unit test
    load_pic()

if __name__ == '__main__':

    # test_version()
    print('task running...')
    from waitress import serve
    serve(app, host='0.0.0.0', port=5004)
    print('task finish...')
    
