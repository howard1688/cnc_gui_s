import cv2
import os
from datetime import datetime
# 初始化相機
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 8000)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 6000)


def take_picture(save_path):
    ret, frame = cap.read()
    if ret:
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        filename = f"photo_{timestamp}.png"
        os.makedirs(save_path, exist_ok=True)
        full_path = os.path.join(save_path, filename)
        # 保存圖片
        cv2.imwrite(full_path, frame)
        print(f"已拍攝照片並保存為： {full_path}")


if __name__ == '__main__':
    save_directory = r"C:\Users\User\Desktop\cnc_gui_s\cnc_gui\camra"
    take_picture(save_directory)
    # 程式結束時釋放相機資源
    cap.release()
    print("相機已釋放，程式已結束")
