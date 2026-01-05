// JavaScript source code
import { JsonModal } from '/components/jsonModal/jsonModal.js';
import '/components/jsonModal/jsonModal.css';
import { ModbusRegisterConfigurator } from '/components/modbus/modbus.js';

const jsonModal = new JsonModal();
export function loadModbusConfigPage() {

    const protocol = {
        "protocol_version": "1.0",
        "data_types": {
            "bit": { "size": 1, "signed": false },
            "int8": { "size": 1, "signed": true },
            "uint8": { "size": 1, "signed": false },
            "int16": { "size": 2, "signed": true },
            "uint16": { "size": 2, "signed": false },
            "float32": { "size": 4, "format": "ieee754" },
            "bitmask16": { "size": 2, "format": "bitmask" }
        },
        "registers": [
            { "address": 0, "type": "int16", "scale": 10.0, "byte_order": "ABCD", "name": "ambient_temp" }
        ],
        "byte_order_presets": { "default": "ABCD" }
    }

    const configurator = new ModbusRegisterConfigurator('modbus-config', protocol);

    const btnAdd = document.createElement('btnAddRegister');
    document.addEventListener('click', (e) => {
        if (e.target.id == 'btnAddRegister') {
            configurator.addRegister();
        } else if (e.target.id == 'btnPrintConfig') {
            try {
                const jsonData = configurator.exportConfig();
                jsonModal.show(jsonData);
            } catch (e) {
            }
        }
    });
}

