# -*- coding: utf-8 -*-

import requests
from bs4 import BeautifulSoup
import json
import logging

# Настройка логирования
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def fetch_currency_data():
    url = 'https://www.cbr.ru/currency_base/daily/'
    try:
        response = requests.get(url)
        response.raise_for_status()  # Проверка на ошибки HTTP
        response.encoding = 'utf-8'
    except requests.RequestException as e:
        logging.error(f"Error fetching data: {e}")
        return []

    soup = BeautifulSoup(response.text, 'html.parser')
    table = soup.find('table', class_='data')

    if not table:
        logging.error("Error: Unable to find the currency table.")
        return []

    currency_list = []
    for row in table.find_all('tr')[1:]:  # Пропускаем заголовок таблицы
        columns = row.find_all('td')
        if columns:
            try:
                currency_list.append({
                    'Code': columns[0].text.strip(),
                    'Code with letters': columns[1].text.strip(),
                    'Denomination': columns[2].text.strip(),
                    'Name': columns[3].text.strip(),
                    'Rate': float(columns[4].text.strip().replace(',', '.'))  # Преобразуем в float
                })
            except (IndexError, ValueError) as e:
                logging.warning(f"Error parsing row: {row}. Error: {e}")

    return currency_list

def save_to_json(data, filename="currencies.json"):
    try:
        with open(filename, "w", encoding="utf-8") as file:
            json.dump(data, file, ensure_ascii=False, indent=4)
        logging.info(f"Data successfully saved to {filename}")
    except Exception as e:
        logging.error(f"Error saving data: {e}")

if __name__ == "__main__":  # Исправлено здесь
    currency_data = fetch_currency_data()
    if currency_data:
        save_to_json(currency_data)
